function CodeBlock(elem)
  if not (FORMAT:match('latex') or FORMAT:match('pdf')) then
    return elem
  end
  elem.text = elem.text:gsub('\\', '\\textbackslash{}')
  return elem
end
