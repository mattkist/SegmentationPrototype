"""Extract plain text from .docx files (word/document.xml)."""
import re
import sys
import zipfile
from pathlib import Path


def extract(path: Path) -> str:
    with zipfile.ZipFile(path) as z:
        xml = z.read("word/document.xml").decode("utf-8")
    text = re.sub(r"</w:p>", "\n", xml)
    text = re.sub(r"<[^>]+>", "", text)
    for old, new in (("&amp;", "&"), ("&lt;", "<"), ("&gt;", ">")):
        text = text.replace(old, new)
    lines = [ln.strip() for ln in text.split("\n") if ln.strip()]
    return "\n".join(lines)


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: extract_docx.py input.docx [output.txt]", file=sys.stderr)
        sys.exit(1)
    inp = Path(sys.argv[1])
    text = extract(inp)
    if len(sys.argv) >= 3:
        Path(sys.argv[2]).write_text(text, encoding="utf-8")
    else:
        sys.stdout.buffer.write(text.encode("utf-8"))
