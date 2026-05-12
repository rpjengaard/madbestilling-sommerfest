# Madbestilling – Sommerfestival

ASP.NET Core + Umbraco app for pre-ordering food at the Sdr. Bjert Skole sommerfestival, with a Tailwind CSS-based frontend.

- **Backend:** ASP.NET Core / Umbraco CMS (see `web.csproj` for target framework)
- **Frontend:** Razor views (`Views/`) styled with Tailwind CSS v4
- **Fonts:** Plus Jakarta Sans + Big Shoulders Display (Google Fonts), Ostrich Sans + Lovelo Line Bold (self-hosted in `wwwroot/fonts/`)

---

## Project layout

```
web/
├── Assets/app.css            # Tailwind source (input)
├── wwwroot/
│   ├── css/app.css           # Tailwind build output (generated)
│   ├── fonts/                # self-hosted fonts (.otf)
│   ├── img/                  # static images (Samvær logo etc.)
│   └── media/                # Umbraco-managed media (do not commit user uploads)
├── Views/                    # Razor pages + partials
├── umbraco/                  # Umbraco runtime data
├── uSync/                    # Umbraco content/schema source-of-truth (committed)
├── package.json              # CSS build scripts
├── Program.cs                # ASP.NET Core entry point
└── web.csproj
```

---

## Local development

### Prerequisites

- .NET SDK matching `web.csproj` `<TargetFramework>` (run `dotnet --info` to verify)
- Node.js 18+ and npm
- SQL Server / LocalDB (or whatever provider is configured in `appsettings.Development.json`)

### First-time setup

```bash
cd web
dotnet restore
npm install
```

### Run dev server

```bash
# Terminal 1 – rebuild CSS on save
npm run css:watch

# Terminal 2 – run the app
dotnet run
```

The app is served at the URL printed by `dotnet run` (typically `https://localhost:5001`). The Umbraco backoffice is at `/umbraco`.

---

## Tailwind CSS

Tailwind v4 is wired up through the `@tailwindcss/cli`. The source file is `Assets/app.css` (imports `tailwindcss`, defines design tokens, custom utilities, `@font-face` declarations). The compiled output is `wwwroot/css/app.css`, which `Views/Shared/_Layout.cshtml` links.

### npm scripts

| Script              | What it does                                                   |
| ------------------- | -------------------------------------------------------------- |
| `npm run css:watch` | Rebuild `wwwroot/css/app.css` on every change to source/views. |
| `npm run css:build` | One-off minified build (use before publishing).                |

> Always run `npm run css:build` before publishing — `dotnet publish` does **not** rebuild Tailwind.

---

## Publishing (production build)

From the `web/` directory:

```bash
# 1. Build a production-minified CSS bundle
npm run css:build

# 2. Publish the .NET app to ./publish
dotnet publish -c Release -o ./publish /p:EnvironmentName=Production
```

The output folder `web/publish/` contains everything the host needs:
- `web.dll` + dependency DLLs
- `web.config` (auto-generated; configures the IIS AspNetCoreModuleV2)
- `wwwroot/` (including the freshly built `css/app.css`)
- Razor views, fonts, images
- `appsettings.json`

Zip it for upload:

```bash
cd publish && zip -r ../publish.zip . && cd ..
```

---

## Deploying to IIS

### 1. One-time server setup

1. Install the **.NET Hosting Bundle** matching the app's target framework: <https://dotnet.microsoft.com/download/dotnet>. This installs the AspNetCoreModuleV2 IIS module.
2. Run `iisreset` (or reboot) after installing.

### 2. Create the site in IIS Manager

1. Create the physical folder, e.g. `C:\inetpub\madbestilling`.
2. Extract `publish.zip` into it.
3. **Add Website**:
   - Physical path: that folder
   - Binding: hostname + port (443 with HTTPS certificate recommended)
4. **Application Pool**:
   - **.NET CLR version → "No Managed Code"** (ASP.NET Core runs out-of-process from IIS' .NET CLR)
   - Identity → must have read access to the site folder, and **read/write** access to:
     - `App_Data/`
     - `umbraco/Data/`
     - `wwwroot/media/`
     - `uSync/` (if you use it on the server)

### 3. Production configuration

Place a `appsettings.Production.json` next to `appsettings.json`, **or** set environment variables via `web.config` / IIS Configuration Editor. Required:

- `ConnectionStrings:umbracoDbDSN` – production SQL Server connection
- `ConnectionStrings:umbracoDbDSN_ProviderName` – e.g. `Microsoft.Data.SqlClient`
- Resend / SMTP credentials (whatever you use for transactional mail)

`ASPNETCORE_ENVIRONMENT=Production` is set automatically by the hosting bundle.

### 4. HTTPS

The app pipeline (in `Program.cs`) includes:

```csharp
app.UseForwardedHeaders(); // honors X-Forwarded-Proto from IIS
app.UseHttpsRedirection();
app.UseHsts();
```

So all HTTP requests are automatically redirected to HTTPS at the application layer. Make sure IIS has an HTTPS binding with a valid certificate.

### 5. Subsequent deploys

**Simple method**: stop the AppPool → overwrite files → start the AppPool.

**Zero-downtime method**: drop an empty `app_offline.htm` in the site root before copying files, then delete it when done.

**Automated method**: Web Deploy (`msdeploy`), Azure DevOps pipeline, or GitHub Actions targeting IIS.

---

## Troubleshooting

- **Fonts not loading (Lovelo/Ostrich)** — make sure `npm run css:build` ran so the `@font-face` rules are present in `wwwroot/css/app.css`. Hard-reload (Cmd/Ctrl+Shift+R) to bypass the browser cache.
- **HTTPS redirect loop behind IIS/proxy** — verify `app.UseForwardedHeaders()` runs *before* `UseHttpsRedirection()` and that IIS is forwarding `X-Forwarded-Proto`.
- **403 on `/umbraco`** — the AppPool identity needs write access to `App_Data/` and `umbraco/Data/`.
- **Pages 404 but `/umbraco` works** — usually an Umbraco published-content issue. Log in, republish, and check `uSync/` is in sync.
