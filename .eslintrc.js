module.exports = {
  "env": {
    "es2021": true,
    "browser": true,
    "commonjs": true,
    "node": true
  },
  "extends": [
    "eslint:recommended"
  ],
  "overrides": [
    {
      "files": [ "**/*.test.js" ],
      "env": {
        "jest": true
      }
    }
  ],
  "parserOptions": {
    "ecmaVersion": "latest"
  },
  "rules": {
    /* possible problems */
    "array-callback-return": "error",
    "no-await-in-loop": "warn",
    "no-constructor-return": "error",
    "no-duplicate-imports": "error",
    "no-promise-executor-return": "error",
    "no-self-compare": "error",
    "no-template-curly-in-string": "error",
    "no-unmodified-loop-condition": "warn",
    "no-unreachable-loop": "warn",
    "no-unused-private-class-members": "warn",
    "no-use-before-define": [ "error", "nofunc" ],
    "require-atomic-updates": [ "error", {
      "allowProperties": false
    } ],

    /* suggestions */
    "accessor-pairs": [ "error", {
      "setWithoutGet": true,
      "getWithoutSet": false,
      "enforceForClassMembers": true
    } ],
    "arrow-body-style": [ "warn", "as-needed" ],
    "block-scoped-var": "error",
    "camelcase": [ "error", {
      "properties": "always",
      "ignoreDestructuring": false,
      "ignoreImports": false,
      "ignoreGlobals": false,
      "allow": [
        "client_id",
        "client_secret",
        "grant_type",
        "token_type",
        "expires_in",
        "access_token",
        "artist_unicode",
        "favourite_count",
        "play_count",
        "preview_url",
        "title_unicode",
        "user_id",
        "download_disabled",
        "more_information",
        "can_be_hyped",
        "discussion_enabled",
        "discussion_locked",
        "is_scoreable",
        "last_updated",
        "legacy_thread_url",
        "ranked_date",
        "submitted_date",
        "grade_counts",
        "hit_accuracy",
        "is_ranked",
        "maximum_combo",
        "play_time",
        "global_rank",
        "ranked_score",
        "replays_watched_by_others",
        "total_hits",
        "total_score",
        "avatar_url",
        "country_code",
        "default_group",
        "is_active",
        "is_bot",
        "is_deleted",
        "is_online",
        "is_supporter",
        "last_visit",
        "pm_friends_only",
        "profile_colour",
        "end_date",
        "mode_specific",
        "participant_count",
        "start_date"
      ] // allow these since osu!api uses these names
    } ],
    "class-methods-use-this": [ "error", {
      "exceptMethods": [],
      "enforceForClassFields": true
    } ],
    "consistent-return": [ "error", {
      "treatUndefinedAsUnspecified": false
    } ],
    "consistent-this": "off",
    "default-case-last": "error",
    "dot-notation": "warn",
    "eqeqeq": "warn",
    "max-classes-per-file": [ "error", {
      "max": 1,
      "ignoreExpressions": false
    } ],
    "multiline-comment-style": [ "warn", "starred-block" ],
    "no-alert": "error",
    "no-confusing-arrow": [ "warn", {
      "allowParens": true
    } ],
    "no-empty-function": "error",
    "no-var": "warn",
    "prefer-const": [ "warn", {
      "destructuring": "any",
      "ignoreReadBeforeAssign": false
    } ],
    "require-await": "error",
    "spaced-comment": [ "warn", "always" ],
    "yoda": "warn",

    /* layout and formatting */
    "array-bracket-newline": [ "warn", "consistent" ],
    "array-bracket-spacing": [ "warn", "always" ],
    "array-element-newline": [ "warn", "consistent" ],
    "arrow-spacing": "warn",
    "block-spacing": [ "warn", "always" ],
    "brace-style": [ "warn", "stroustrup" ],
    "comma-dangle": [ "warn", "never" ],
    "comma-spacing": [ "warn", {
      "before": false,
      "after": true
    } ],
    "dot-location": [ "warn", "property" ],
    "eol-last": [ "warn", "always" ],
    "func-call-spacing": [ "warn", "never" ],
    "implicit-arrow-linebreak": [ "warn", "beside" ],
    "indent": [ "warn", 2, {
      "SwitchCase": 1
    } ],
    "jsx-quotes": [ "warn", "prefer-double" ],
    "key-spacing": [ "warn", {
      "beforeColon": false,
      "afterColon": true,
      "mode": "strict"
    } ],
    "keyword-spacing": [ "warn", {
      "before": true,
      "after": true,
      "overrides": {
        "if": {
          "after": false
        },
        "for": {
          "after": false
        },
        "while": {
          "after": false
        },
        "super": {
          "after": false
        },
        "switch": {
          "after": false
        }
      }
    } ],
    "lines-around-comment": [ "warn", {
      "beforeBlockComment": true,
      "beforeLineComment": true,
      "allowBlockStart": true,
      "allowObjectStart": true
    } ],
    "no-mixed-spaces-and-tabs": "error",
    "no-multi-spaces": [ "warn", {
      "ignoreEOLComments": false,
      "exceptions": { "Property": true }
    } ],
    "no-multiple-empty-lines": [ "warn", {
      "max": 1,
      "maxEOF": 1,
      "maxBOF": 0
    } ],
    "no-trailing-spaces": "warn",
    "no-whitespace-before-property": "warn",
    "object-curly-newline": [ "warn", {
      "ObjectExpression": {
        "multiline": true,
        "minProperties": 2,
        "consistent": true
      },
      "ObjectPattern": { "multiline": false },
      "ImportDeclaration": { "multiline": false },
      "ExportDeclaration": { "multiline": false }
    } ],
    "object-curly-spacing": [ "warn", "always" ],
    "operator-linebreak": [ "warn", "none" ],
    "padded-blocks": [ "warn", "never" ],
    "quotes": [ "warn", "double" ],
    "rest-spread-spacing": "warn",
    "semi": [ "warn", "always" ],
    "semi-spacing": [ "warn", {
      "before": false,
      "after": true
    } ],
    "space-infix-ops": "warn",
    "switch-colon-spacing": "warn",
    "template-curly-spacing": [ "warn", "always" ],
    "template-tag-spacing": [ "warn", "always" ],

    /* typescript-eslint/eslint-plugin */
    "@typescript-eslint/explicit-module-boundary-types": "off"
  }
};
