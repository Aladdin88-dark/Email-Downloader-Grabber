<div align="center">

# 📧 Email Grabber Pro

### Enterprise Email & Security Analysis Suite

[![RU](https://img.shields.io/badge/README-🇷🇺_Русский-0057a8?style=for-the-badge)](./README.ru.md)
&nbsp;
[![Telegram](https://img.shields.io/badge/💬_Contact-Telegram-2CA5E0?style=for-the-badge&logo=telegram)](https://t.me/YOUR_TELEGRAM_HERE)

---

![Windows](https://img.shields.io/badge/Windows-10_%7C_11-0078D4?style=flat-square&logo=windows)
![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-13.0-239120?style=flat-square&logo=csharp)
![License](https://img.shields.io/badge/License-Commercial-DC143C?style=flat-square)
![IMAP](https://img.shields.io/badge/IMAP_Servers-3_000_000%2B-f57c00?style=flat-square)
![Status](https://img.shields.io/badge/Status-Active-00C851?style=flat-square)

</div>

---

<div align="center">

| 📡 3,000,000+ IMAP servers | 🔑 4 crypto key types | 🌐 Works without proxies |
|:---:|:---:|:---:|
| **📷 On-device OCR** | **👁️ ~1 KB per photo** | **⚡ 3,000 threads** |

</div>

---

## 📦 Modules

<table>
<tr>
<td width="50%" valign="top">

### 📬 IMAP Email Checker
Bulk credential validation at scale.
```
✔  No proxy required — direct connection
✔  3,000,000+ built-in IMAP server mappings
✔  Auto-resolves server from email domain
✔  Socks5 / HTTP proxy + per-account rotation
✔  Re-check queue for failed accounts
✔  Up to 3,000 parallel threads
✔  Real-time output → Valid_Email.txt
```

</td>
<td width="50%" valign="top">

### 🔵 Hotmail / Outlook Checker
Microsoft account audit via Web API.
```
✔  Inbox · Sent · Drafts download
✔  Attachment extraction with filters
✔  Built-in BIP-39 seed phrase scanner
✔  Private key scanner (Hex · EVM · WIF)
✔  Download emails by keyword
✔  Date-range filters
✔  Up to 3,000 sessions + download pool
```

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 📥 IMAP Mix Email
Deep forensic harvesting from any mailbox.
```
✔  Inbox · Sent · Deleted in parallel
✔  Extension whitelist (pdf jpg zip xlsx…)
✔  Seed phrase / private key detection
✔  Download emails by keyword
✔  Sender / subject / date filters
✔  Works without proxies
```

</td>
<td width="50%" valign="top">

### ☁️ OneDrive Auditor
Cloud storage enumeration via Microsoft API.
```
✔  4 modes: extension / keyword / both / API
✔  File size cap
✔  Seed / key scanner on all content
✔  Up to 45 parallel download threads
✔  Works without proxies
```

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 📷 Photo Seed & Key Scanner
Crypto credentials from images — **fully offline OCR**.
```
✔  No internet · no API key
✔  jpg png heic heif webp bmp tiff gif avif
✔  OCR: EN FR ES IT PT JP KO ZH CS
✔  BIP-39 seeds (12–24 words, checksum)
✔  Hex keys · EVM keys · WIF keys
✔  Up to 64 parallel workers
✔  Seed.txt · Hex.txt · Evm.txt · Wif.txt
```

</td>
<td width="50%" valign="top">

### 👁️ Anti-Public Image Engine
Detects unique images — **no photos stored on disk**.
```
✔  No proxies · no internet needed
✔  Only hash fingerprints saved (~1 KB/image)
✔  Original photos NEVER stored
✔  6 signals: SHA-256 · pHash×5 · dHash
             · aHash · edge-pHash · Histogram
✔  Rotation invariant: 0° 90° 180° 270° + flip
✔  Auto-moves Anti-Public images to Result\
```

</td>
</tr>
</table>

---

## 👁️ Anti-Public — How It Works

| Signal | Algorithm | Resistant To |
|:---:|:---|:---|
| **SHA-256** | Exact file hash | — |
| **pHash ×5** | DCT hash in 5 orientations | Resize · JPEG · rotation · flip |
| **dHash** | Horizontal gradient hash | Brightness / contrast |
| **aHash** | Average hash | Global structure changes |
| **edge-pHash** | pHash on Canny edges | Colour filters · toning |
| **Histogram** | Bhattacharyya coefficient | Any spatial transformation |

```
RUN 1 — BUILD MODE
  ▸ Feed your "known / public" photo library
  ▸ Only hash fingerprints saved (~1 KB per image)
  ▸ Nothing is moved

RUN 2+ — DETECTION MODE
  ▸ KNOWN    (found in DB) → stays in place
  ▸ UNKNOWN  (not in DB)  → Anti-Public → moved to Result\
```

**Detection rules — first match wins:**

```
Rule 1  SHA-256 exact match
Rule 2  best pHash ≤ 8  (all 5 rotation/flip variants)
Rule 3  pHash ≤ 14  AND  dHash ≤ 10  AND  aHash ≤ 10
Rule 4  edge-pHash ≤ 6  AND  histogram BC ≥ 0.95
Rule 5  dHash ≤ 5  AND  aHash ≤ 5  AND  histogram BC ≥ 0.97
```

---

## 💳 Pricing

<div align="center">

| Plan | Price | Duration | Includes |
|:---:|:---:|:---:|:---|
| 🗓️ **Weekly** | **$50** | 7 days | All 6 modules · Full functionality |
| 📅 **Monthly** | **$80** | 30 days | All 6 modules · Priority support |
| ♾️ **Lifetime** | **$200** | Forever | All 6 modules · All future updates |

<br/>

### 💬 To purchase — write on Telegram: [`@YOUR_TELEGRAM_HERE`](https://t.me/YOUR_TELEGRAM_HERE)

</div>

---

## 🚀 Quick Start

```
1. Download the latest release (Releases tab)
2. Extract to any folder — no installation needed
3. Run Email Grabber.exe
4. Enter your license key
```

Optional config files next to the executable:

```
imap_accounts.txt   — email:password list
imap_servers.txt    — server overrides  (3M+ built-in, usually not needed)
proxies.txt         — proxy list  (optional — works without proxies)
```

---

## 📁 Output Structure

```
app-folder/
├── AntiPublic/
│   ├── phash_database.txt        ← permanent hash DB, never cleared
│   └── AntiPublic_features.txt
└── Result/
    ├── Valid_Email.txt
    ├── Hotmail_DD_MM_YYYY/
    ├── IMAP_DD_MM_YYYY/
    ├── OneDrive_DD_MM_YYYY/
    ├── PhotoSeed&PK_DD_MM_YYYY/
    │   └── Seed.txt · Hex.txt · Evm.txt · Wif.txt
    └── Anti-Public_DD_MM_YYYY/
```

---

## 💻 Requirements

| | |
|:---|:---|
| **OS** | Windows 10 (22621+) or Windows 11 |
| **Runtime** | .NET 9 — bundled in release |
| **CPU** | x64 · 4+ cores |
| **RAM** | 4 GB min · 8 GB recommended |
| **Proxy** | Optional — direct connection supported |

---

## 🔧 Stack

`C# 13` · `.NET 9` · `WPF` · `ModernWpf` · `MailKit` · `Magick.NET` · `Windows.Media.Ocr` · `NBitcoin`

---

## ⚖️ Legal

> For **authorized** security research, corporate IT audits and lawful OSINT only.  
> You must have explicit permission before using this tool on systems you do not own.  
> Authors accept no liability for misuse.

---

<div align="center">

### 💬 [`@YOUR_TELEGRAM_HERE`](https://t.me/YOUR_TELEGRAM_HERE)

*Built for security professionals · Windows 10/11 · .NET 9*

</div>

