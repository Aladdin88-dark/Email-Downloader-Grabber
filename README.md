<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:0f0c29,100:302b63&height=200&section=header&text=Email%20Grabber%20Pro&fontSize=52&fontColor=ffffff&fontAlignY=38&desc=Enterprise%20Email%20%26%20Security%20Analysis%20Suite&descSize=16&descAlignY=60&descColor=aaaaff" width="100%" />

<br/>

[![Lang](https://img.shields.io/badge/README-🇷🇺%20Русский-blue?style=for-the-badge)](./README.ru.md)

<br/>

<img src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4?style=flat-square&logo=windows&logoColor=white"/>
<img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet&logoColor=white"/>
<img src="https://img.shields.io/badge/C%23-13.0-239120?style=flat-square&logo=csharp&logoColor=white"/>
<img src="https://img.shields.io/badge/License-Commercial-DC143C?style=flat-square"/>
<img src="https://img.shields.io/badge/IMAP%20Servers-3%2C000%2C000%2B-orange?style=flat-square"/>
<img src="https://img.shields.io/badge/Status-Active-00C851?style=flat-square"/>

<br/><br/>

> **High-performance Windows desktop suite** for IMAP validation, email forensics,  
> cloud storage audit, crypto key recovery and perceptual image fingerprinting.  
> Built for security professionals and enterprise IT teams.

<br/>

[Key Features](#-key-features) · [Modules](#-modules) · [Anti-Public](#-anti-public--deep-dive) · [Pricing](#-pricing) · [Quick Start](#-quick-start)

</div>

---

## ⚡ Key Features

<div align="center">

| | |
|:---|:---|
| 🌐 **Works without proxies** | Direct connection mode — no proxy required. Add proxies only when needed for rotation or anonymity. |
| 📡 **3,000,000+ IMAP servers** | Built-in database covers virtually every mail domain on the planet. No manual configuration needed. |
| 🌱 **Seed phrase extraction from emails** | Scans email bodies and attachments for BIP-39 mnemonic phrases and private keys automatically. |
| 📷 **Seed phrase extraction from photos** | On-device OCR reads seed phrases and private keys directly from image files — no internet, no API. |
| 📩 **Download emails by keyword** | Filter and download emails by sender, subject or custom keywords across any date range. |
| 👁️ **Anti-Public without storing photos** | The engine stores only compact hash fingerprints (~1 KB per image), not the photos themselves. |

</div>

---

## 🧩 Modules

<table>
<tr>
<td width="50%" valign="top">

### 📬 IMAP Email Checker
Bulk credential validation against IMAP servers.

- **Works without proxies** — direct connection supported
- **3,000,000+ built-in server mappings** — auto-resolves any domain
- Socks5 / HTTP proxy with per-account rotation
- Re-check queue on network errors
- Up to **3 000 parallel threads**
- Real-time output → `Valid_Email.txt`

</td>
<td width="50%" valign="top">

### 🔵 Hotmail / Outlook Checker
Microsoft account audit via Web API.

- Inbox · Sent · Drafts download
- Attachment extraction with size & extension filters
- **Built-in seed-phrase / private key scanner** in email bodies
- Download emails **by keyword** (sender / subject filters)
- Up to **3 000 sessions** + separate download thread pool

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 📥 IMAP Mix Email
Deep attachment harvesting from any mailbox.

- Scans Inbox · Sent · Deleted simultaneously
- Extension whitelist (pdf, jpg, zip, xlsx, docx…)
- **Seed / key detection** inside attachments and email bodies
- **Keyword-based email download** with date-range filter

</td>
<td width="50%" valign="top">

### ☁️ OneDrive Auditor
Cloud storage enumeration via Microsoft Live API.

- 4 sub-modes: extension / keyword / both / API search
- File size cap to skip irrelevant large files
- **Integrated seed / key scanner** on all downloaded content
- Up to **45 download threads**

</td>
</tr>
<tr>
<td width="50%" valign="top">

### 📷 Photo Seed & Key Scanner
Crypto credential extraction from images via **on-device OCR**.

> No internet required. No API key. Fully offline.

- Formats: `jpg` `png` `heic` `heif` `webp` `bmp` `tiff` `gif` `avif`
- OCR languages: EN · FR · ES · IT · PT · JP · KO · ZH · CS
- Detects: **BIP-39 seed phrases** · Hex keys · EVM keys · WIF keys
- Checksum validation — only valid seeds are reported
- Up to **64 parallel workers**
- Results: `Seed.txt` · `Hex.txt` · `Evm.txt` · `Wif.txt`

</td>
<td width="50%" valign="top">

### 👁️ Anti-Public Image Engine
Detects unique / unseen images without storing the photos locally.

> **Only compact hash fingerprints are stored (~1 KB per image).**  
> Original photos never need to be kept on disk.

- 6-signal detection: SHA-256 · pHash×5 · dHash · aHash · edge-pHash · Histogram
- Rotation & flip invariant (0° / 90° / 180° / 270° + mirror)
- Works without proxies or internet connection
- Auto-moves Anti-Public images to dated result folder

</td>
</tr>
</table>

---

## 👁️ Anti-Public — Deep Dive

<div align="center">

| Signal | Algorithm | Resistant to |
|:---:|:---|:---|
| **SHA-256** | Exact file hash | — |
| **pHash ×5** | DCT perceptual hash in 5 orientations | Resize · JPEG · format convert · rotation · flip |
| **dHash** | Horizontal gradient hash | Brightness / contrast |
| **aHash** | Average hash | Global structure changes |
| **edge-pHash** | pHash on Canny edges | Colour filters · heavy toning |
| **Histogram** | Bhattacharyya coefficient | Any spatial transformation |

</div>

```
┌─────────────────────────────────────────────────────────────┐
│  RUN 1 — BUILD MODE                                         │
│  ▸ Scan your "known / public" photo library                 │
│  ▸ Only hash fingerprints are saved (~1 KB per image)       │
│  ▸ Original photos are NOT stored or moved                  │
├─────────────────────────────────────────────────────────────┤
│  RUN 2+ — DETECTION MODE                                    │
│  ▸ Each new image is compared against the hash database     │
│  ▸ KNOWN   → stays in place                                 │
│  ▸ UNKNOWN → Anti-Public → moved to Result folder           │
└─────────────────────────────────────────────────────────────┘
```

**Detection rules** — any single match classifies image as KNOWN:

```
Rule 1  SHA-256 exact match
Rule 2  best pHash ≤ 8    (all 5 rotation/flip variants)
Rule 3  pHash ≤ 14   AND  dHash ≤ 10   AND  aHash ≤ 10
Rule 4  edge-pHash ≤ 6   AND  histogram BC ≥ 0.95
Rule 5  dHash ≤ 5    AND  aHash ≤ 5    AND  histogram BC ≥ 0.97
```

---

## 💳 Pricing

<div align="center">

| Plan | Price | Duration | Modules |
|:---:|:---:|:---:|:---|
| 🗓️ **Weekly** | **$50** | 7 days | All 6 modules |
| 📅 **Monthly** | **$80** | 30 days | All 6 modules |
| ♾️ **Lifetime** | **$200** | Forever | All 6 modules + all future updates |

<br/>

📩 **To purchase or ask questions — contact via Telegram**

</div>

---

## 🚀 Quick Start

```bash
# 1. Download the latest release from the Releases tab
# 2. Extract to any folder — no installation required
# 3. Run the executable
Email Grabber.exe
# 4. Enter your license key on first launch
```

**Optional files next to the executable:**

```
imap_servers.txt    — override IMAP server mappings (3M+ built-in)
imap_accounts.txt   — email:password list
proxies.txt         — proxy list (optional — works without proxies)
```

---

## 📄 Input Formats

<details>
<summary><code>imap_accounts.txt</code></summary>

```
user@gmail.com:password123
user@outlook.com:mypassword
```

</details>

<details>
<summary><code>imap_servers.txt</code> — three formats supported (optional override)</summary>

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
<summary><code>proxies.txt</code> — optional, works without proxies</summary>

```
192.168.1.1:1080
user:pass@192.168.1.2:1080
socks5://user:pass@proxy.example.com:1080
http://user:pass@proxy.example.com:8080
```

</details>

---

## 📁 Output Structure

```
app-folder/
│
├── AntiPublic/                          ← permanent hash DB, never cleared
│   ├── phash_database.txt               ← ~1 KB per image, no photos stored
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
    │   ├── Seed.txt  ├── Hex.txt  ├── Evm.txt  └── Wif.txt
    └── Anti-Public_DD_MM_YYYY_HH_MM_SS/
```

---

## 💻 System Requirements

| | |
|:---|:---|
| **OS** | Windows 10 (22621+) · Windows 11 |
| **Runtime** | .NET 9 — bundled in release |
| **CPU** | x64 · 4+ cores recommended |
| **RAM** | 4 GB min · 8 GB recommended |
| **Proxy** | Optional — direct connection supported |
| **OCR** | Windows OCR packs via Settings → Language |

---

## 🔧 Stack

<div align="center">

`C# 13` · `.NET 9` · `WPF` · `ModernWpf` · `MailKit` · `Magick.NET` · `Windows.Media.Ocr` · `NBitcoin` · `System.Text.Json`

</div>

---

## ⚖️ Legal

> This software is for **authorized** security research and IT administration only.  
> You must have explicit authorization before using it against systems you do not own.  
> The authors accept no liability for misuse.

---

<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=0:302b63,100:0f0c29&height=100&section=footer" width="100%"/>

</div>
