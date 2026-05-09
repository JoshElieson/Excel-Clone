# Spreadsheet Application

[Spreadsheet site LIVE](https://excel-clone-eosin.vercel.app/)

A spreadsheet application inspired by Excel, originally developed for **CS 3500 (Software Practice)** at the University of Utah. The **core engine** (formulas, dependency graph, XML save/load) remains **C#**; a **React** UI and **ASP.NET Core** API were added so it can run in the browser and be deployed publicly.

## What’s in this repo

| Part | Description |
|------|-------------|
| **`Spreadsheet`**, **`Formula`**, **`DependencyGraph`**, etc. | Original class libraries used by both the MAUI app and the API. |
| **`GUI`** | Original **.NET MAUI** desktop/mobile spreadsheet (26×99 grid, XML I/O, text colors). |
| **`SpreadsheetApi`** | **ASP.NET Core 8** minimal API that exposes the same `Spreadsheet` behavior over HTTP (`/api/...`). |
| **`web`** | **React + TypeScript + Vite** front end: grid, formula bar, New/Open/Save, help, color swatches. |
| **`Dockerfile`** / **`.dockerignore`** | Container build for hosting **`SpreadsheetApi`** (e.g. on Render). |

The web client does **not** reimplement the spreadsheet logic in JavaScript; it calls the API, which uses the existing C# implementation.

## Features (web + API)

- Excel-like grid (A1–Z99), contents vs. displayed values, formulas with `=`
- Dependency-aware recalculation and error handling (e.g. circular references, bad formulas)
- XML **`.sprd`**-style save/load compatible with the original spreadsheet format (version `six`)
- Optional per-cell text color (stored in the browser for the web UI)

## Formula examples

```txt
=A1 + B2
=C5 * 2
=(A1 + A2) / B1
```

## Deployment (how this project is set up to run online)

- **Frontend:** **`web`** is deployed on **Vercel** (root directory **`web`**, build **`npm run build`**, output **`dist`**).
- **Backend:** **`SpreadsheetApi`** is deployed as a **Docker** service (e.g. **Render**) using the repo-root **`Dockerfile`**. Render sets **`PORT`**; the container listens on **`0.0.0.0:$PORT`**.
- **Connecting them:** **`web/vercel.json`** rewrites same-origin **`/api/:path*`** to the live API, e.g. **`https://<your-service>.onrender.com/api/:path*`**. That way the browser only talks to your Vercel domain for `/api` requests; Vercel forwards them to Render.

