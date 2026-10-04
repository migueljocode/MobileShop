# MobileShop
ASP.NET Core inventory & sales system for a local mobile phone shop, with IMEI-tracked stock and invoice generation.

## Money

- Monetary values are stored as whole **IRR (Rial)** amounts in `long` fields.
- The application enforces `MoneyLimits.MaxRials` as the maximum supported money value.
- Web displays use grouped digits followed by **IRR** (for example, `1,234,567 IRR`).
- Factor PDFs keep the Persian **ریال** unit.
- Seed data is expressed in Rials.
- Existing Production rows are assumed to already be in Rials and are **not automatically converted**.
