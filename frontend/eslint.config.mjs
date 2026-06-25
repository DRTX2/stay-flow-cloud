import next from "eslint-config-next";

/**
 * Flat ESLint config. `eslint-config-next` v16 ships a native flat-config array
 * (core-web-vitals + TypeScript rules), so it is spread in directly.
 */
const eslintConfig = [
  {
    ignores: [".next/**", "node_modules/**", "coverage/**", "playwright-report/**"],
  },
  ...next,
];

export default eslintConfig;
