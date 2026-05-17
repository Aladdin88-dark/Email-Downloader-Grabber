<div align="center">

<svg width="800" height="180" xmlns="http://www.w3.org/2000/svg">
  <defs>
    <linearGradient id="bg" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="#0f0c29"/>
      <stop offset="50%" stop-color="#302b63"/>
      <stop offset="100%" stop-color="#1a1a4e"/>
    </linearGradient>
  </defs>
  <rect width="800" height="180" fill="url(#bg)" rx="14"/>
  <rect x="0" y="0" width="800" height="3" fill="#7b7bff" rx="2"/>
  <rect x="0" y="177" width="800" height="3" fill="#7b7bff" rx="2"/>
  <text x="400" y="88" font-family="Arial,sans-serif" font-size="46" font-weight="bold" fill="#ffffff" text-anchor="middle" dominant-baseline="middle">Email Grabber Pro</text>
  <text x="400" y="138" font-family="Arial,sans-serif" font-size="16" fill="#aaaaee" text-anchor="middle">Enterprise Email &amp; Security Analysis Suite</text>
  <line x1="300" y1="116" x2="500" y2="116" stroke="#7b7bff" stroke-width="1" opacity="0.6"/>
</svg>

<br/>

[![RU](https://img.shields.io/badge/README-🇷🇺%20Русский-0057a8?style=for-the-badge)](./README.ru.md)
&nbsp;
[![Telegram](https://img.shields.io/badge/Contact-@YOUR__TELEGRAM__HERE-2CA5E0?style=for-the-badge&logo=telegram&logoColor=white)](https://t.me/YOUR_TELEGRAM_HERE)

</div>

<br/>

<div align="center">

<img src="https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011-0078D4?style=flat-square&logo=windows&logoColor=white"/>
&nbsp;
<img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white"/>
&nbsp;
<img src="https://img.shields.io/badge/Language-C%23%2013-239120?style=flat-square&logo=csharp&logoColor=white"/>
&nbsp;
<img src="https://img.shields.io/badge/License-Commercial-DC143C?style=flat-square"/>
&nbsp;
<img src="https://img.shields.io/badge/IMAP%20Database-3%2C000%2C000%2B%20servers-f57c00?style=flat-square"/>
&nbsp;
<img src="https://img.shields.io/badge/Status-Active%20Development-00C851?style=flat-square"/>

</div>

<br/>

<div align="center">

```
╔══════════════════════════════════════════════════════════════════════════╗
║  6 professional modules  ·  3M+ IMAP servers  ·  No proxy required     ║
║  On-device OCR  ·  Crypto forensics  ·  Anti-Public image engine        ║
╚══════════════════════════════════════════════════════════════════════════╝
```

</div>

---

<div align="center">

## At a Glance

|   |   |   |
|:---:|:---:|:---:|
| 📡 **3,000,000+** | 🔑 **4 key types** | 🌐 **Proxy-free** |
| Built-in IMAP server database | Seed · Hex · EVM · WIF | Works on direct connection |
| 📷 **On-device OCR** | 👁️ **~1 KB per photo** | ⚡ **3,000 threads** |
| No API key, fully offline | Anti-Public stores only hashes | Max parallelism |

</div>

---

## 📦 Modules

<table>
<tr>
<td width="50%" valign="top">

### 📬 IMAP Email Checker

Validates email credentials against IMAP servers at massive scale.

```
✔  Works without proxies (direct connection)
✔  3,000,000+ built-in server mappings
✔  Auto-resolves server from email domain
✔  Socks5 / HTTP proxy with per-account rotation
✔  Re-check queue for failed accounts
✔  Up to 3,000 parallel threads
✔  Real-time streaming → Valid_Email.txt
```

</td>
<td width="50%" valign="top">

### 🔵 Hotmail / Outlook Checker

Microsoft account audit via Web API.

```
✔  Inbox · Sent · Drafts simultaneous download
✔  Attachment extraction (size + extension filters)
✔  Built-in BIP-39 seed phrase scanner
✔  Built-in private key scanner (Hex · EVM · WIF)
✔  Download emails by keyword (sender / subject)
✔  Date-range filters
✔  Up to 3,000 sessions + download thread pool
```

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 📥 IMAP Mix Email

Deep forensic harvesting from any IMAP mailbox.

```
✔  Inbox · Sent · Deleted Items in parallel
✔  Extension whitelist (pdf jpg png zip xlsx…)
✔  Seed phrase / private key detection
✔  Download emails by keyword
✔  Sender / subject / date-range filters
✔  Works without proxies
```

</td>
<td width="50%" valign="top">

### ☁️ OneDrive Auditor

Cloud storage enumeration via Microsoft Live API.

```
✔  4 sub-modes: extension / keyword / both / API
✔  File size cap to skip large irrelevant files
✔  Seed phrase / private key scanner on results
✔  Up to 45 parallel download threads
✔  Works without proxies
```

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 📷 Photo Seed & Key Scanner

Extracts cryptocurrency credentials from images using **on-device OCR**.

```
✔  No internet · no API key · fully offline
✔  jpg png heic heif webp bmp tiff gif avif
✔  OCR: EN FR ES IT PT JP KO ZH CS + more
✔  BIP-39 seed phrases (12–24 words, checksum)
✔  Hex private keys (64-char)
✔  EVM keys (0x-prefixed)
✔  WIF keys (5 / K / L prefix)
✔  Up to 64 parallel workers
✔  Real-time: Seed.txt Hex.txt Evm.txt Wif.txt
```

</td>
<td width="50%" valign="top">

### 👁️ Anti-Public Image Engine

Detects unique / previously unseen images using **6-signal perceptual fingerprinting**.

```
✔  Works without proxies or internet
✔  Stores only hash fingerprints (~1 KB/image)
✔  Original photos are NEVER stored on disk
✔  6 signals: SHA-256 · pHash×5 · dHash
            · aHash · edge-pHash · Histogram
✔  Rotation invariant: 0° 90° 180° 270° + flip
✔  Persistent hash database survives cleanup
✔  Auto-moves Anti-Public images to Result folder
```

</td>
</tr>
</table>

---

## 👁️ Anti-Public — How It Works

<div align="center">

| Signal | What It Does | Resistant To |
|:---:|:---|:---|
| **SHA-256** | Exact byte-level file match | — |
| **pHash ×5** | DCT perceptual hash in 5 orientations | Resize · JPEG · format conversion · rotation · flip |
| **dHash** | Horizontal gradient comparison | Brightness · contrast adjustments |
| **aHash** | Global average luminance hash | Overall structural changes |
| **edge-pHash** | pHash applied to Canny edge map | Colour grading · heavy filters · toning |
| **Histogram** | Bhattacharyya colour distribution | All spatial transformations |

</div>

<br/>

```
  ┌──────────────────────────────────────────────────────────────────────┐
  │  RUN 1 — BUILD MODE                                                  │
  │  ▸ Feed your "known / public" library                                │
  │  ▸ Only hash fingerprints saved  (~1 KB per image)                   │
  │  ▸ Original photos untouched. Nothing is moved.                      │
  ├──────────────────────────────────────────────────────────────────────┤
  │  RUN 2+ — DETECTION MODE                                             │
  │  ▸ KNOWN (found in DB)    → stays in place                           │
  │  ▸ ANTI-PUBLIC (not in DB) → moved to Result\ folder                 │
  └──────────────────────────────────────────────────────────────────────┘
```

**Detection rules** — first match wins:

```
  Rule 1  SHA-256 exact file match
  Rule 2  best pHash ≤ 8     across all 5 rotation/flip variants
  Rule 3  pHash ≤ 14   AND   dHash ≤ 10   AND   aHash ≤ 10
  Rule 4  edge-pHash ≤ 6    AND   histogram BC ≥ 0.95
  Rule 5  dHash ≤ 5    AND   aHash ≤ 5    AND   histogram BC ≥ 0.97
```

---

## 💳 Pricing

<div align="center">

<table>
<tr>
<td align="center" width="33%">

### 🗓️ Weekly
# $50
**7 days**

All 6 modules  
Full functionality  
Email support

</td>
<td align="center" width="33%">

### 📅 Monthly
# $80
**30 days**

All 6 modules  
Full functionality  
Priority support

</td>
<td align="center" width="33%">

### ♾️ Lifetime
# $200
**Forever**

All 6 modules  
All future updates  
Priority support

</td>
</tr>
</table>

<br/>

> 💬 **To purchase a license or ask any question:**
>
> ### 📩 [`@YOUR_TELEGRAM_HERE`](https://t.me/YOUR_TELEGRAM_HERE)

</div>

---

## 🚀 Quick Start

**1.** Download the latest release from the [Releases](../../releases) tab

**2.** Extract to any folder — portable, no installation required

**3.** Run `Email Grabber.exe`

**4.** Enter your license key on first launch

<br/>

Optional files next to the executable:

```
imap_servers.txt    — custom server overrides  (3M+ built-in, usually not needed)
imap_accounts.txt   — email:password list
proxies.txt         — proxy list              (optional — works without proxies)
```

---

## 📄 Input File Formats

<details>
<summary><b>imap_accounts.txt</b></summary>
<br/>

```
user@gmail.com:password
user@outlook.com:password
```

</details>

<details>
<summary><b>imap_servers.txt</b> — three formats supported</summary>
<br/>

```ini
# Tag-based
[DOMAINS]gmail.com[/DOMAINS][SERVER]imap.gmail.com[/SERVER][PORT]993[/PORT][SSL]true[/SSL]

# INI key=value
outlook.com = imap-mail.outlook.com:993

# Whitespace-separated
yahoo.com   imap.mail.yahoo.com   993
```

</details>

<details>
<summary><b>proxies.txt</b> — optional</summary>
<br/>

```
192.168.1.1:1080
user:pass@192.168.1.2:1080
socks5://user:pass@proxy.example.com:1080
```

</details>

---

## 📁 Output Structure

```
app-folder/
│
├── AntiPublic/                        ← permanent hash DB (never cleared)
│   ├── phash_database.txt             ← ~1 KB per image, no photos stored
│   └── AntiPublic_features.txt
│
└── Result/
    ├── Valid_Email.txt
    ├── Hotmail_DD_MM_YYYY_HH_MM_SS/
    │   ├── emails/
    │   └── attachments/
    ├── IMAP_DD_MM_YYYY_HH_MM_SS/
    ├── OneDrive_DD_MM_YYYY_HH_MM_SS/
    ├── PhotoSeed&PK_DD_MM_YYYY_HH_MM_SS/
    │   ├── Seed.txt · Hex.txt · Evm.txt · Wif.txt
    └── Anti-Public_DD_MM_YYYY_HH_MM_SS/
```

---

## 💻 System Requirements

| Requirement | Details |
|:---|:---|
| **OS** | Windows 10 (build 22621+) or Windows 11 |
| **Runtime** | .NET 9 — bundled inside the release |
| **CPU** | x64 · 4+ cores recommended |
| **RAM** | 4 GB min · 8 GB recommended |
| **Proxy** | Optional — direct connection supported |
| **OCR** | Windows OCR packs via Settings → Language |

---

## 🔧 Technology Stack

<div align="center">

`C# 13` · `.NET 9` · `WPF + ModernWpf` · `MailKit` · `Magick.NET` · `Windows.Media.Ocr` · `NBitcoin` · `System.Text.Json`

</div>

---

## ⚖️ Legal Notice

> This software is intended exclusively for **authorized** security research, penetration testing, corporate IT audits and lawful OSINT operations.  
> You must hold explicit written permission before using this tool against any system you do not own.  
> The authors accept no liability for misuse.

---

<div align="center">

**Questions? Ready to purchase?**

### 📩 [`@YOUR_TELEGRAM_HERE`](https://t.me/YOUR_TELEGRAM_HERE)

<br/>

*Built for security professionals · Windows 10 / 11 · .NET 9*

</div>

