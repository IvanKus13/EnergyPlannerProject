import js from "@eslint/js";
import globals from "globals";

export default [
    js.configs.recommended,
    {
        files: ["EnergyPlanner/wwwroot/**/*.js"],
        languageOptions: {
            ecmaVersion: 2022,
            sourceType: "script",
            globals: { ...globals.browser }
        },
        rules: {
            "no-unused-vars": "error",
            "no-undef": "error",
            "eqeqeq": ["error", "always"],
            "no-var": "error",
            "prefer-const": "warn",
            "no-console": "off"
        }
    }
];
