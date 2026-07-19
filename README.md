# AI-Assisted Professional Engineering with .NET

AI-Assisted Professional Engineering with .NET is a bilingual (English/Arabic) technical book that explores how Artificial Intelligence can be integrated into professional software engineering practices across the .NET ecosystem.

The project focuses on treating AI as an engineering collaborator rather than a replacement for engineering judgment. The book combines software architecture, system design, development workflows, testing, operations, governance, and AI-assisted engineering practices into a unified reference for modern .NET professionals.

## Why This Book Exists

Artificial Intelligence is changing how software is designed, implemented, tested, and maintained.

Many resources focus on AI tools in isolation. This book takes a different approach.

The objective is to examine how AI can be integrated into professional engineering processes while preserving architectural discipline, maintainability, quality standards, and long-term system evolution.

The book is written for engineers who want to understand not only how to use AI tools, but also how to incorporate them responsibly into real-world software engineering environments.

## Intended Audience

This book is primarily intended for:

- Senior Software Engineers
- Technical Leads
- Solution Architects
- Software Architects
- Engineering Managers

It may also be useful for developers and technology professionals interested in AI-assisted software engineering practices.

## Reading Options

### English

- [Introduction](./book/manifesto/introduction.en.md)
- [Table of Contents](./TOC.en.md)

### العربية

- [المقدمة](./book/manifesto/introduction.ar.md)
- [جدول المحتويات](./TOC.ar.md)

## Publication Model

The manuscript is published incrementally.

Publication units are organized as:

| Unit | Approximate Size |
|--------|--------|
| Section | ~2,000 words |
| Chapter | ~15,000 words |

New sections are published progressively as they are completed, reviewed, and prepared for publication.

## Current Project Status

The repository is currently in its bootstrap publication phase.

Current public content:

- Introduction
- Table of Contents
- Project governance documentation
- Publication policies
- Publication roadmap

Additional content will be published incrementally.

For details see:

- [Project Status](./docs/project-status/project-status.en.md)
- [Roadmap](./docs/roadmap/roadmap.en.md)

## Project Principles

The project is guided by a set of engineering principles that define its long-term direction.

Key themes include:

- Engineering judgment remains essential
- Architecture precedes automation
- Maintainability outweighs short-term productivity
- Reproducibility matters
- Transparency builds trust

See:

- [Project Principles](./docs/governance/project-principles.en.md)

## Publishing Framework

Project publication follows a documented process.

Relevant documents:

- [Publishing Strategy](./docs/publishing/publishing-strategy.en.md)
- [Release Policy](./docs/publishing/release-policy.en.md)
- [Versioning Policy](./docs/versioning/versioning-policy.en.md)

## Community Participation

Community feedback is welcome.

At the current stage of the project, source-code and manuscript contributions are intentionally limited while the publication structure, engineering model, and long-term direction continue to mature.

See:

- [Community Guidelines](./COMMUNITY_GUIDELINES.md)
- [Contributing Guidelines](./CONTRIBUTING.md)

## Repository Structure

```text
.
├── book
│   ├── chapters
│   ├── manifesto
│   └── samples
│
├── docs
│   ├── governance
│   ├── marketing
│   ├── project-status
│   ├── publishing
│   ├── roadmap
│   └── versioning
│
├── exports
│   ├── docx
│   └── pdf
│
├── scripts
│   └── export
│
├── TOC.en.md
├── TOC.ar.md
└── README.md
```

## Building Local Exports

Export scripts are included in the repository to allow generation of local PDF and DOCX outputs directly from the Markdown sources.

Export tooling is documented and versioned alongside the manuscript to ensure reproducibility.

## License

This project is licensed under the MIT License.

See:

- [LICENSE](./LICENSE)

## Project Repository

Author: Tarek Najem

GitHub Repository:

https://github.com/TarekNajem04/AI-Assisted-Professional-Engineering-with-.NET