import { execFileSync } from "node:child_process";
import { createWriteStream } from "node:fs";
import PDFDocument from "pdfkit";
import { appendFileSync, existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const root = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const reportsDir = join(root, "reports");
const sarifPath = join(reportsDir, "dotnet-analysis.sarif");
const eslintJsonPath = join(reportsDir, "eslint-report.json");
const eslintHtmlPath = join(reportsDir, "eslint-report.html");
const summaryPath = join(reportsDir, "summary.md");
const pdfPath = join(reportsDir, "static-analysis-report.pdf");
const fontRegular = join(root, "node_modules", "dejavu-fonts-ttf", "ttf", "DejaVuSans.ttf");
const fontBold = join(root, "node_modules", "dejavu-fonts-ttf", "ttf", "DejaVuSans-Bold.ttf");

mkdirSync(reportsDir, { recursive: true });

function run(command, args) {
    try {
        execFileSync(command, args, { cwd: root, stdio: "inherit", shell: process.platform === "win32" });
    } catch {
        // Помилки лінтера не зупиняють скрипт: результат рахується зі звітів нижче.
    }
}

run("dotnet", [
    "build", "EnergyPlanner/EnergyPlanner.csproj",
    "--no-incremental", "--nologo", "-v", "q",
    `-p:ErrorLog=${sarifPath}`
]);

const eslint = join(root, "node_modules", ".bin", process.platform === "win32" ? "eslint.cmd" : "eslint");
run(eslint, ["EnergyPlanner/wwwroot", "--format", "json", "--output-file", eslintJsonPath]);
run(eslint, ["EnergyPlanner/wwwroot", "--format", "html", "--output-file", eslintHtmlPath]);

let dotnetIssues = [];
if (existsSync(sarifPath)) {
    const sarif = JSON.parse(readFileSync(sarifPath, "utf8"));
    for (const runResult of sarif.runs ?? []) {
        for (const result of runResult.results ?? []) {
            if (result.ruleId?.startsWith("CA") || result.ruleId?.startsWith("IDE")) {
                dotnetIssues.push(result);
            }
        }
    }
}

let eslintErrors = 0;
let eslintWarnings = 0;
const eslintIssues = [];
if (existsSync(eslintJsonPath)) {
    for (const file of JSON.parse(readFileSync(eslintJsonPath, "utf8"))) {
        eslintErrors += file.errorCount;
        eslintWarnings += file.warningCount;
        for (const message of file.messages) {
            const shortPath = file.filePath.replace(root, "").replace(/^[\\/]+/, "");
            eslintIssues.push(`${message.ruleId ?? "eslint"}: ${message.message} (${shortPath}:${message.line})`);
        }
    }
}

const normalizedRoot = root.replace(/\\/g, "/").replace(/^\/+/, "");
const dotnetIssueLines = [];
for (const issue of dotnetIssues) {
    const location = issue.locations?.[0]?.resultFile ?? issue.locations?.[0]?.physicalLocation;
    const uri = decodeURIComponent(location?.uri ?? location?.artifactLocation?.uri ?? "");
    const file = uri.replace(/^file:\/+/, "").replace(normalizedRoot, "").replace(/^\/+/, "");
    dotnetIssueLines.push(`${issue.ruleId}: ${issue.message?.text ?? issue.message ?? ""} (${file})`);
}

const total = dotnetIssues.length + eslintErrors + eslintWarnings;
const status = total === 0 ? "✅ Зауважень немає" : `❌ Знайдено зауважень: ${total}`;

const lines = [
    "# Звіт статичного аналізу",
    "",
    `Дата: ${new Date().toISOString()}`,
    "",
    "| Інструмент | Набір правил | Зауважень |",
    "|---|---|---|",
    `| .NET Code Analyzers | latest-recommended | ${dotnetIssues.length} |`,
    `| ESLint | eslint.config.mjs | ${eslintErrors} помилок, ${eslintWarnings} попереджень |`,
    "",
    `**${status}**`,
    "",
    "Повні звіти: `reports/dotnet-analysis.sarif`, `reports/eslint-report.html`.",
    ""
];

for (const issue of dotnetIssueLines) {
    lines.push(`- ${issue}`);
}

for (const issue of eslintIssues) {
    lines.push(`- ${issue}`);
}

const summary = lines.join("\n");
writeFileSync(summaryPath, summary, "utf8");
console.log("\n" + summary);

if (process.env.GITHUB_STEP_SUMMARY) {
    appendFileSync(process.env.GITHUB_STEP_SUMMARY, summary + "\n");
}

await writePdf();

process.exitCode = total === 0 ? 0 : 1;

function writePdf() {
    return new Promise((resolvePdf, rejectPdf) => {
        const doc = new PDFDocument({ size: "A4", margin: 50, info: { Title: "Звіт статичного аналізу EnergyPlanner" } });
        const stream = createWriteStream(pdfPath);
        stream.on("finish", resolvePdf);
        stream.on("error", rejectPdf);
        doc.pipe(stream);

        doc.registerFont("regular", fontRegular);
        doc.registerFont("bold", fontBold);

        doc.font("bold").fontSize(18).text("Звіт статичного аналізу EnergyPlanner");
        doc.moveDown(0.3);
        doc.font("regular").fontSize(10).fillColor("#555555")
            .text(`Згенеровано: ${new Date().toLocaleString("uk-UA", { timeZone: "Europe/Kyiv" })}`);
        doc.fillColor("#000000").moveDown(1);

        const rows = [
            ["Інструмент", "Набір правил", "Зауважень"],
            [".NET Code Analyzers", "latest-recommended", String(dotnetIssues.length)],
            ["ESLint", "eslint.config.mjs", `${eslintErrors} помилок, ${eslintWarnings} попереджень`]
        ];
        const columns = [170, 150, 175];
        for (let r = 0; r < rows.length; r++) {
            const y = doc.y;
            let x = doc.page.margins.left;
            for (let c = 0; c < columns.length; c++) {
                doc.rect(x, y, columns[c], 22).stroke("#999999");
                doc.font(r === 0 ? "bold" : "regular").fontSize(10).fillColor("#000000")
                    .text(rows[r][c], x + 6, y + 6, { width: columns[c] - 12 });
                x += columns[c];
            }
            doc.x = doc.page.margins.left;
            doc.y = y + 22;
        }

        doc.moveDown(1.2);
        doc.font("bold").fontSize(13).fillColor(total === 0 ? "#1b7f3b" : "#b42318")
            .text(total === 0 ? "Зауважень немає" : `Знайдено зауважень: ${total}`, doc.page.margins.left);
        doc.fillColor("#000000");

        if (total > 0) {
            doc.moveDown(0.6);
            doc.font("bold").fontSize(11).text("Перелік зауважень");
            doc.font("regular").fontSize(9);
            for (const line of [...dotnetIssueLines, ...eslintIssues]) {
                doc.text(`• ${line}`, { width: 495 });
            }
        }

        doc.moveDown(1.2);
        doc.font("regular").fontSize(9).fillColor("#555555")
            .text("Повні звіти інструментів: reports/dotnet-analysis.sarif, reports/eslint-report.html. "
                + "Згенеровані міграції EF аналізатор пропускає автоматично.", { width: 495 });

        doc.end();
    });
}
