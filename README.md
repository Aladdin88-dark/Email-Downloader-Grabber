<div align="center">

<h1>📧 Email Grabber Pro</h1>
<h3>Enterprise Email &amp; Security Analysis Suite</h3>

<p>
  <a href="./README.ru.md"><img src="https://img.shields.io/badge/README-🇷🇺%20Русский-1a56b0?style=for-the-badge" /></a>
  &nbsp;
  <a href="https://t.me/cyberpaladin"><img src="https://img.shields.io/badge/💬%20Buy%20%2F%20Contact-Telegram-2CA5E0?style=for-the-badge&logo=telegram&logoColor=white" /></a>
</p>

<p>
  <img src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?style=flat-square&logo=windows&logoColor=white" />
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/C%23-13.0-239120?style=flat-square&logo=csharp&logoColor=white" />
  <img src="https://img.shields.io/badge/License-GPL%20v3-DC143C?style=flat-square" />
  <img src="https://img.shields.io/badge/IMAP%20Servers-3%2C000%2C000%2B-f57c00?style=flat-square" />
  <img src="https://img.shields.io/badge/Status-Active-00C851?style=flat-square" />
  <img src="https://img.shields.io/github/v/release/Aladdin88-dark/Email-Downloader-Grabber?style=flat-square&label=Release&color=00C851" />
</p>

<p><i>High-performance Windows desktop suite for IMAP validation, email forensics,<br/>cloud storage audit, crypto key recovery and perceptual image fingerprinting.</i></p>

<img src="Assets/Marketing/banner.png" width="850" />

</div>

---

<div align="center">

| 📡 **3,000,000+** built-in IMAP servers | 🔑 Seed · Hex · EVM · WIF detection | 🌐 Works **without proxies** |
|:---:|:---:|:---:|
| 📷 **On-device OCR** — no API, no internet | 👁️ Anti-Public stores only **~1 KB per photo** | ⚡ Up to **3,000 threads** |

</div>

---

## 📦 Modules

<table>
<tr>
<td width="50%" valign="top">

### 📬 IMAP Email Checker
Bulk credential validation against IMAP servers.

```
✔  Works without proxies (direct connection)
✔  3,000,000+ built-in IMAP server mappings
✔  Auto-resolves server from email domain
✔  Socks5 / HTTP proxy + per-account rotation
✔  Re-check queue for network errors
✔  Up to 3,000 parallel threads
✔  Real-time output → Valid_Email.txt
```

</td>
<td width="50%" valign="top">

### 🔵 Hotmail / Outlook Checker
Microsoft account audit via Web API.

```
✔  Inbox · Sent · Drafts simultaneous download
✔  Attachment extraction (size + extension filters)
✔  Built-in BIP-39 seed phrase scanner
✔  Private key scanner (Hex · EVM · WIF)
✔  Download emails by keyword (sender / subject)
✔  Date-range filters
✔  Up to 3,000 sessions + download thread pool
```

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 📥 IMAP Mix Email
Deep forensic harvesting from any mailbox.

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
✔  4 modes: extension / keyword / both / API search
✔  File size cap to skip irrelevant files
✔  Seed / key scanner on all downloaded content
✔  Up to 45 parallel download threads
✔  Works without proxies
```

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 📷 Photo Seed & Key Scanner
Crypto credentials from images — **on-device OCR**.

```
✔  No internet required · no API key · offline
✔  jpg png heic heif webp bmp tiff gif avif
✔  OCR: EN FR ES IT PT JP KO ZH CS + more
✔  BIP-39 seed phrases (12–24 words, checksum)
✔  Hex private keys (64-char)
✔  EVM keys (0x-prefixed)
✔  WIF keys (5 / K / L prefix)
✔  Up to 64 parallel workers
✔  Seed.txt · Hex.txt · Evm.txt · Wif.txt
```

</td>
<td width="50%" valign="top">

### 👁️ Anti-Public Image Engine
Detects unique images using **6-signal fingerprinting**.

```
✔  No proxies · no internet required
✔  Only hash fingerprints saved (~1 KB/image)
✔  Original photos are NEVER stored on disk
✔  6 signals:
    SHA-256 · pHash×5 · dHash
    aHash · edge-pHash · Histogram
✔  Rotation invariant: 0° 90° 180° 270° + flip
✔  Persistent DB — survives Result folder cleanup
✔  Auto-moves Anti-Public images to Result\
```

</td>
</tr>
</table>

---

## 👁️ Anti-Public — Deep Dive

<div align="center">

| Signal | Algorithm | Resistant To |
|:---:|:---|:---|
| **SHA-256** | Exact byte-level file hash | — |
| **pHash ×5** | DCT perceptual hash in 5 orientations | Resize · JPEG · format convert · rotation · flip |
| **dHash** | Horizontal gradient hash | Brightness / contrast adjustments |
| **aHash** | Average luminance hash | Global structure changes |
| **edge-pHash** | pHash on Canny edge map | Colour grading · heavy filters |
| **Histogram** | Bhattacharyya colour coefficient | Any spatial transformation |

</div>

```
┌─────────────────────────────────────────────────────────────────┐
│  RUN 1 — BUILD MODE                                             │
│                                                                 │
│  ▸ Feed your "known / public" photo library                     │
│  ▸ Only hash fingerprints saved  (~1 KB per image)              │
│  ▸ Original photos are untouched. Nothing is moved.             │
├─────────────────────────────────────────────────────────────────┤
│  RUN 2+ — DETECTION MODE                                        │
│                                                                 │
│  ▸ KNOWN    (found in DB)  →  stays in place                    │
│  ▸ UNKNOWN  (not in DB)    →  Anti-Public  →  moved to Result\  │
└─────────────────────────────────────────────────────────────────┘
```

**Detection rules — first match wins:**

```
Rule 1  SHA-256 exact file match
Rule 2  best pHash ≤ 8     (all 5 rotation/flip variants)
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
## $50
**7 days**

All 6 modules  
Full functionality  
Email support

</td>
<td align="center" width="33%">

### 📅 Monthly
## $80
**30 days**

All 6 modules  
Full functionality  
Priority support

</td>
<td align="center" width="33%">

### ♾️ Lifetime
## $200
**Forever**

All 6 modules  
All future updates  
Priority support

</td>
</tr>
</table>

<br/>

> ### 💬 To purchase or ask a question → [`@cyberpaladin`](https://t.me/cyberpaladin)

</div>

---

## 🧪 Demo Version

A **free demo** is available in this repository with limited functionality:

| Feature | Demo | Professional |
|:---|:---:|:---:|
| IMAP valid check (Mode M) | ✅ up to 50 threads | ✅ up to 3,000 threads |
| Hotmail valid check (Mode H) | ✅ up to 50 threads | ✅ up to 3,000 threads |
| Email & attachment download | ❌ | ✅ |
| Seed phrase / key scanner | ❌ | ✅ |
| OneDrive, Anti-Public, Photo OCR | ❌ | ✅ |

[**→ Download Demo**](https://github.com/Aladdin88-dark/Email-Downloader-Grabber/releases)

---

## 📸 Screenshots

<div align="center">
<img src="Assets/Screenshots/main.png" width="820" />
</div>

<details>
<summary><b>More screenshots</b></summary>
<br/>
<div align="center">
<img src="Assets/Screenshots/imap.png" width="820" />
<br/><br/>
<img src="Assets/Screenshots/hotmail.png" width="820" />
</div>
</details>

---

## 🚀 Quick Start

**1.** Download the latest release from the **[Releases](https://github.com/Aladdin88-dark/Email-Downloader-Grabber/releases)** tab

**2.** Extract to any folder — portable, no installation required

**3.** Run `Email Grabber.exe` and enter your license key

<br/>

<details>
<summary><b>📄 Configuration files (click to expand)</b></summary>
<br/>

**`imap_accounts.txt`**
```
user@gmail.com:password
user@outlook.com:password
```

**`imap_servers.txt`** — 3M+ built-in, custom override optional
```ini
# Tag-based
[DOMAINS]gmail.com[/DOMAINS][SERVER]imap.gmail.com[/SERVER][PORT]993[/PORT][SSL]true[/SSL]
# INI format
outlook.com = imap-mail.outlook.com:993
```

**`proxies.txt`** — optional, works without proxies
```
192.168.1.1:1080
user:pass@host:1080
socks5://user:pass@proxy.example.com:1080
```

</details>

---

## 📁 Output Structure

```
app-folder/
│
├── AntiPublic/                         ← permanent hash DB, never cleared
│   ├── phash_database.txt              ← ~1 KB per image, no photos stored
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

| | |
|:---|:---|
| **OS** | Windows 10 (build 22621+) or Windows 11 |
| **Runtime** | .NET 9 — bundled inside the release binary |
| **CPU** | x64 · 4+ cores recommended |
| **RAM** | 4 GB minimum · 8 GB recommended |
| **Proxy** | Optional — direct connection fully supported |
| **OCR** | Windows OCR packs (Settings → Time & Language → Language) |

---

## 🔧 Technology Stack

<div align="center">

| Layer | Technology |
|:---:|:---|
| **Language** | C# 13 on .NET 9 |
| **UI** | WPF + ModernWpf (Windows 11 design language) |
| **IMAP** | MailKit |
| **Image processing** | Magick.NET (ImageMagick) |
| **OCR** | Windows.Media.Ocr — on-device, zero latency, no API key |
| **Crypto** | NBitcoin — BIP-39 checksum validation |
| **Config** | System.Text.Json |

</div>

---

## ⚖️ Legal Notice

> This software is intended exclusively for **authorized** security research, penetration testing, corporate IT audits, and lawful OSINT operations.  
> You must hold explicit written permission before using this tool against any system you do not own.  
> The authors accept no liability for misuse.

---

<div align="center">

**Questions? Ready to purchase?**

### 💬 [`@cyberpaladin`](https://t.me/cyberpaladin)

<br/>

*Built for security professionals · Windows 10 / 11 · .NET 9*

</div>
