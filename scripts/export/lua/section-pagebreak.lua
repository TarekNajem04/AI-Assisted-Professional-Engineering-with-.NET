-- ============================================================================
-- section-pagebreak.lua
--
-- Purpose
-- -------
-- Start every top-level section (Markdown H1) on a new page in assembled
-- chapter exports, in both DOCX and PDF.
--
-- The first H1 is left untouched: a single-section export must not get a
-- blank page after the cover, and the first section of an assembled chapter
-- already follows the cover page.
--
-- Formats
-- -------
--   DOCX  -> raw OpenXML page break paragraph
--   PDF   -> \newpage (LaTeX)
-- ============================================================================

local firstH1Seen = false

function Header(el)
    if el.level ~= 1 then
        return nil
    end

    if not firstH1Seen then
        firstH1Seen = true
        return nil
    end

    if FORMAT:match("docx") then
        return {
            pandoc.RawBlock("openxml", '<w:p><w:r><w:br w:type="page"/></w:r></w:p>'),
            el
        }
    end

    if FORMAT:match("latex") or FORMAT:match("pdf") then
        return {
            pandoc.RawBlock("latex", "\\newpage"),
            el
        }
    end

    return nil
end