# LocalizationManager Configuration Guide

This guide explains all available configuration options for LocalizationManager (lrm).

## Configuration File

Create a file named `lrm.json` in the same directory as your resource files. You can copy and customize `lrm-sample.json` as a starting point.

## Priority Order

Configuration values are resolved in the following priority order:

1. **Command-line arguments** (highest priority)
2. **Configuration file** (`lrm.json`)
3. **Environment variables** (for API keys)
4. **Default values** (lowest priority)

---

## Configuration Options

### General Settings

#### `defaultLanguageCode`

- **Type:** `string`
- **Default:** `null` (displays as "default")
- **Description:** The language code to display for the default language (e.g., "en", "fr"). This only affects display output, not internal logic.
- **Example:**
  ```json
  "defaultLanguageCode": "en"
  ```

---

### Translation Settings

Configuration for the machine translation feature.

#### `translation.defaultProvider`

- **Type:** `string`
- **Default:** `"google"`
- **Allowed values:** `"google"`, `"deepl"`, `"libretranslate"`
- **Description:** The default translation provider to use when not explicitly specified.
- **Example:**
  ```json
  "defaultProvider": "deepl"
  ```

#### `translation.defaultSourceLanguage`

- **Type:** `string`
- **Default:** `null` (auto-detect)
- **Description:** The default source language for translation. Use ISO 639-1 language codes (e.g., "en", "es", "fr", "de"). If not set, the provider will attempt to auto-detect.
- **Example:**
  ```json
  "defaultSourceLanguage": "en"
  ```

#### `translation.maxRetries`

- **Type:** `integer`
- **Default:** `3`
- **Description:** Maximum number of retry attempts for failed translation requests.
- **Example:**
  ```json
  "maxRetries": 5
  ```

#### `translation.timeoutSeconds`

- **Type:** `integer`
- **Default:** `30`
- **Description:** Timeout in seconds for translation requests.
- **Example:**
  ```json
  "timeoutSeconds": 60
  ```

#### `translation.batchSize`

- **Type:** `integer`
- **Default:** `10`
- **Description:** Number of keys to translate in a single batch request. Larger batches are faster but may exceed provider limits.
- **Example:**
  ```json
  "batchSize": 20
  ```

#### `translation.useSecureCredentialStore`

- **Type:** `boolean`
- **Default:** `false`
- **Description:** Whether to use the secure credential store for API keys. When enabled, API keys can be stored encrypted in the user's application data directory. Use the `lrm config` command to manage credentials securely.
- **Example:**
  ```json
  "useSecureCredentialStore": true
  ```

---

### API Keys

**⚠️ WARNING:** Do not commit API keys to version control! Add `lrm.json` to `.gitignore` if it contains keys.

**Priority order for API keys:**

1. Environment variables (most secure)
2. Secure credential store (if enabled)
3. Configuration file values (least secure)

#### `translation.apiKeys.google`

- **Type:** `string`
- **Environment variable:** `LRM_GOOGLE_API_KEY`
- **Description:** Google Cloud Translation API key
- **Get your key:** https://cloud.google.com/translate/docs/setup
- **Example:**
  ```json
  "google": "AIzaSyD..."
  ```

#### `translation.apiKeys.deepL`

- **Type:** `string`
- **Environment variable:** `LRM_DEEPL_API_KEY`
- **Description:** DeepL API key
- **Get your key:** https://www.deepl.com/pro-api
- **Example:**
  ```json
  "deepL": "abc123..."
  ```

#### `translation.apiKeys.libreTranslate`

- **Type:** `string`
- **Environment variable:** `LRM_LIBRETRANSLATE_API_KEY`
- **Description:** LibreTranslate API key (optional for public instances)
- **Learn more:** https://libretranslate.com/
- **Example:**
  ```json
  "libreTranslate": "xyz789..."
  ```

---

### Code Scanning Settings

Configuration for the code scanning feature that detects localization key usage in source files.

#### `scanning.resourceClassNames`

- **Type:** `array of strings`
- **Default:** `["Resources", "Strings", "AppResources"]`
- **Description:** Resource class names to detect in code. These are the class names whose property accesses will be detected as localization key references.
- **Detected patterns:**
  - C#: `Resources.KeyName`, `AppResources.WelcomeMessage`
  - XAML: `{x:Static res:Strings.PageTitle}`
  - Razor: `@Resources.ErrorMessage`
- **Example:**
  ```json
  "resourceClassNames": [
    "Resources",
    "Strings",
    "AppResources",
    "Loc"
  ]
  ```

#### `scanning.localizationMethods`

- **Type:** `array of strings`
- **Default:** `["GetString", "GetLocalizedString", "Translate", "L", "T"]`
- **Description:** Localization method names to detect. These are method names that accept localization keys as string parameters.
- **Detected patterns:**
  - `GetString("KeyName")`
  - `Translate("WelcomeMessage")`
  - `L("ErrorText")`
  - `T("ButtonLabel")`
- **Example:**
  ```json
  "localizationMethods": [
    "GetString",
    "Translate",
    "L",
    "T",
    "GetLocalizedString",
    "Loc"
  ]
  ```

---

## Complete Example

```json
{
  "defaultLanguageCode": "en",
  "translation": {
    "defaultProvider": "deepl",
    "defaultSourceLanguage": "en",
    "maxRetries": 5,
    "timeoutSeconds": 60,
    "batchSize": 15,
    "useSecureCredentialStore": true,
    "apiKeys": {
      "google": "",
      "deepL": "",
      "libreTranslate": ""
    }
  },
  "scanning": {
    "resourceClassNames": [
      "Resources",
      "Strings",
      "AppResources",
      "Loc"
    ],
    "localizationMethods": [
      "GetString",
      "GetLocalizedString",
      "Translate",
      "L",
      "T",
      "Localize"
    ]
  }
}
```

---

## Security Best Practices

### API Key Management

1. **Use Environment Variables (Recommended)**
   ```bash
   export LRM_GOOGLE_API_KEY="your-key-here"
   export LRM_DEEPL_API_KEY="your-key-here"
   ```

2. **Use Secure Credential Store**
   ```bash
   lrm config set-api-key google "your-key-here"
   lrm config set-api-key deepl "your-key-here"
   ```

3. **Avoid Storing in Configuration File**
   - If you must store keys in `lrm.json`, ensure it's in `.gitignore`
   - Never commit API keys to version control

### .gitignore Entry

Add this to your `.gitignore`:

```
lrm.json
```

While still keeping the sample file tracked:

```
lrm.json
!lrm-sample.json
```

---

## Command-Line Overrides

Most configuration options can be overridden via command-line arguments. For example:

```bash
# Override translation provider
lrm translate --provider deepl --target-language es

# Override scanning configuration
lrm scan --source-path ./src --resource-classes "Loc,Resources"
```

Command-line arguments always take priority over configuration file settings.
