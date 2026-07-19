-- ============================================================================
-- codeblock-bidi.lua
--
-- Jeninnet / Professional Book Export Pipeline
--
-- Purpose
-- -------
-- Normalize code blocks before they reach any rendering engine.
-- Enforce left alignment via direction attribute (DOCX) and raw LaTeX (PDF).
--
-- Supported targets
--
--   • DOCX   → direction=“ltr” attribute
--   • PDF    → \begin{flushleft}\textdir TLT… wrapping
--   • HTML   → direction=“ltr” attribute
--
-- ============================================================================

local List = require("pandoc.List")

-----------------------------------------------------------------------
-- Helpers
-----------------------------------------------------------------------

local function hasArabic(text)

    return text:match("[%z\1-\127\194-\244][\128-\191]*")

           and text:match("[\216-\219][\128-\191]")

end

local function splitLines(text)

    local lines = {}

    for line in (text .. "\n"):gmatch("(.-)\n") do
        table.insert(lines, line)
    end

    return lines

end

-----------------------------------------------------------------------
-- Detect Arabic inside code block
-----------------------------------------------------------------------

local function containsArabic(lines)

    for _, line in ipairs(lines) do

        if hasArabic(line) then
            return true
        end

    end

    return false

end

-----------------------------------------------------------------------
-- Preserve original code (no reordering)
-----------------------------------------------------------------------

local function normalize(lines)

    local out = List:new()

    for _, line in ipairs(lines) do
        out:insert(line)
    end

    return table.concat(out, "\n")

end

-----------------------------------------------------------------------
-- Main
-----------------------------------------------------------------------

function CodeBlock(block)

    local lines = splitLines(block.text)

    -------------------------------------------------------------------
    -- Never modify source code.
    -------------------------------------------------------------------

    block.text = normalize(lines)

    -------------------------------------------------------------------
    -- Store metadata.
    -------------------------------------------------------------------

    block.attributes["contains-arabic"] =
        containsArabic(lines) and "true" or "false"

    block.attributes["direction"] = "ltr"

    -------------------------------------------------------------------
    -- PDF / LaTeX output: wrap code block in flushleft + LTR direction
    -- This overrides the document-wide RTL for code blocks only.
    -------------------------------------------------------------------

    if FORMAT:match("latex") or FORMAT:match("pdf") then

        local prefix =
            "\\begin{flushleft}" ..
            "\\textdir TLT\\pardir TLT\\bodydir TLT{}"

        local suffix = "\\end{flushleft}"

        return {
            pandoc.RawBlock("latex", prefix),
            block,
            pandoc.RawBlock("latex", suffix)
        }

    end

    return block

end