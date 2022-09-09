import fg from "fast-glob";

// import nodePolyfills from 'rollup-plugin-polyfill-node';
import { nodeResolve } from '@rollup/plugin-node-resolve';
import { brotliCompressSync } from "zlib";
import commonjs from "@rollup/plugin-commonjs";
import resolve from "@rollup/plugin-node-resolve";
import babel from "@rollup/plugin-babel";
import scss from "rollup-plugin-scss";
import copy from "rollup-plugin-copy";
import { terser } from "rollup-plugin-terser";
import gzipPlugin from "rollup-plugin-gzip";
import monaco from 'rollup-plugin-monaco-editor';
import postcss from 'rollup-plugin-postcss';

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
      input: "./client/js/MonacoEdit.ts",
      output: [
        {
          dir: `src/Xenial.Identity.Components/wwwroot/js/MonacoEdit`,
          format: 'esm',
          sourcemap: debug,
          plugins: debug ? [] : [terser()],
        },
      ],
      external: [],
      plugins: [
        // nodePolyfills({
        //   include: ["util", "stream", "zlib", "assert", "buffer"]
        // }),
        resolve({ extensions }),
        scss({
          output: `src/Xenial.Identity.Components/wwwroot/css/MonacoEdit-bundle.css`,
          outputStyle: debug ? undefined : "compressed",
        }),
        postcss({
          url: (asset) => {
            if (!/\.ttf$/.test(asset.url)) return asset.url;
            const distPath = path.join(process.cwd(), 'dist');
            const distFontsPath = path.join(distPath, 'fonts');
            fs.ensureDirSync(distFontsPath);
            const targetFontPath = path.join(distFontsPath, asset.pathname);
            fs.copySync(asset.absolutePath, targetFontPath);
            const relativePath = path.relative(process.cwd(), targetFontPath);
            const publicPath = '/';
            return `${publicPath}${relativePath}`;
          },
        }),
        monaco({
          languages: ['json', 'html', 'css'],
        }),
        nodeResolve(),
        commonjs(),
        babel({
          extensions,
          exclude: "node_modules/**",
          babelHelpers: 'bundled'
        })
      ]
    },
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
