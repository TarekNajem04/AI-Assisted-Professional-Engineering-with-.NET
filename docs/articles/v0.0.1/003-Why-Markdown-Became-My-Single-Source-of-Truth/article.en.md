# Why Markdown Became the Single Source of Truth?

[Medium](https://tareknajem04.medium.com/ai-assisted-professional-engineering-with-net-62fa8a5921b9)
[LinkedIn](https://www.linkedin.com/pulse/ai-assisted-professional-engineering-net-tarek-najem-uizje/)

One architectural decision changed this project more than any other.

It wasn't choosing .NET.

It wasn't selecting Pandoc.

It wasn't deciding how the book would be published.

It was deciding **where the truth should live**.

Every long-term project eventually faces the same problem.

The number of outputs keeps growing.

PDFs.

Word documents.

HTML pages.

Translated editions.

Printed books.

Website articles.

Presentation slides.

Sooner or later, someone edits one version and forgets to update another.

The documentation slowly begins to contradict itself.

The problem isn't poor documentation.

The problem is that there are multiple sources of truth.

I wanted to eliminate that risk before it appeared.

That led to one simple architectural principle:

> **Write once. Generate everything else.**

Markdown became the project's Single Source of Truth.

Every chapter is written once.

Every heading exists once.

Every code sample has one authoritative version.

Every diagram is referenced from one location.

Every explanation is maintained in one place.

Everything else is generated.

This decision had consequences far beyond publishing.

Git became easier to use because text changes are meaningful.

Reviews became smaller and more focused.

Automation became deterministic.

Localization became manageable because translators work from a stable source instead of exported documents.

Most importantly, the knowledge became independent of its presentation.

A PDF is not the truth.

A DOCX file is not the truth.

A website is not the truth.

They are simply different representations of the same knowledge.

That distinction changes how an engineering project evolves.

Once the content is separated from its presentation, publishing becomes an implementation detail rather than the center of the system.

Today the platform generates professional documents.

Tomorrow it may generate websites, interactive documentation, or formats that don't even exist yet.

The manuscript will remain exactly where it belongs.

At the center of the architecture.

Looking back, choosing Markdown was never really about Markdown.

It was about protecting knowledge from the constant evolution of tools.

Technologies change.

Publishing platforms change.

File formats change.

Well-structured knowledge should not.

---

## Engineering Series

Previous

[**← 002-Why I Built My Own Markdown Publishing Platform?**](../002-Why-I-Built-My-Own-Markdown-Publishing-Platform/article.en.md)

Next

[**004-Building a Bilingual Technical Manuscript →**](../004-Building-a-Bilingual-Technical-Manuscript/article.en.md)
