import fg from "fast-glob";

import { brotliCompressSync } from "zlib";
import commonjs from "@rollup/plugin-commonjs";
import resolve from "@rollup/plugin-node-resolve";
import babel from "@rollup/plugin-babel";
import scss from "rollup-plugin-scss";
import copy from "rollup-plugin-copy";
import { terser } from "rollup-plugin-terser";
import gzipPlugin from "rollup-plugin-gzip";

const extensions = [".js", ".ts"];

const additionalFiles = () => [
  `./src/Xenial.Identity/wwwroot/css/bundle.css`,
  ...fg.sync("./src/Xenial.Identity/wwwroot/css/*.svg"),
  ...fg.sync("./src/Xenial.Identity/wwwroot/css/*.ttf"),
  ...fg.sync("./src/Xenial.Identity/wwwroot/img/*.svg"),
  ...fg.sync("./src/Xenial.Identity/wwwroot/img/**/*.ico"),
];

export default (commandLineArgs) => {
  const debug = commandLineArgs.configDebug;

  return [
    {
      input: "./client/js/index.ts",
      output: [
        {
          file: `src/Xenial.Identity/wwwroot/js/index.min.js`,
          format: "iife",
          plugins: debug ? [] : [terser()],
        },
      ],
      external: [],
      plugins: [
        resolve({ extensions }),
        commonjs(),
        babel({
          extensions,
          exclude: "node_modules/**",
          babelHelpers: 'bundled'
        }),
        scss({
          output: `src/Xenial.Identity/wwwroot/css/bundle.css`,
          outputStyle: debug ? undefined : "compressed",
        }),
        debug
          ? undefined
          : copy({
            targets: [
              {
                src: "node_modules/@xenial-io/xenial-template/src/css/*.woff",
                dest: "./src/Xenial.Identity/wwwroot/css",
              },
              {
                src:
                  "node_modules/@xenial-io/xenial-template/src/css/*.woff2",
                dest: "./src/Xenial.Identity/wwwroot/css",
              },
              {
                src: "node_modules/@xenial-io/xenial-template/src/css/*.ttf",
                dest: "./src/Xenial.Identity/wwwroot/css",
              },
              {
                src: "node_modules/@xenial-io/xenial-template/src/css/*.svg",
                dest: "./src/Xenial.Identity/wwwroot/css",
              },
              {
                src: "node_modules/@xenial-io/xenial-template/src/img/**/*",
                dest: "./src/Xenial.Identity/wwwroot/img",
              },
            ],
          }),
        debug
          ? undefined
          : copy({
            targets: [
              {
                src: "client/img",
                dest: "./src/Xenial.Identity/wwwroot",
              }
            ],
            flatten: false,
          }),
        debug
          ? undefined
          : gzipPlugin({
            additionalFiles: additionalFiles(),
          }),
        debug
          ? undefined
          : gzipPlugin({
            additionalFiles: additionalFiles(),
            customCompression: (content) =>
              brotliCompressSync(Buffer.from(content)),
            fileName: ".br",
          })
      ],
    },
  ];
};
