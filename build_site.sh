#!/usr/bin/env bash
set -euo pipefail

SRC_DIR="./source"
SITE_DIR="./"

if [[ ! -d "$SRC_DIR" ]]; then
  echo "Source directory '$SRC_DIR' does not exist"
  exit 1
fi

echo "Creating Jekyll site structure..."

mkdir -p "$SITE_DIR"
mkdir -p "$SITE_DIR/_layouts"

# -----------------------------
# Jekyll config
# -----------------------------
cat > "$SITE_DIR/_config.yml" <<'EOF'
title: Markdown Site
theme: minima
markdown: kramdown
EOF

# -----------------------------
# Default layout
# -----------------------------
cat > "$SITE_DIR/_layouts/default.html" <<'EOF'
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <title>{{ page.title }} - {{ site.title }}</title>
  <meta name="viewport" content="width=device-width, initial-scale=1">
</head>
<body>
  <main style="max-width: 800px; margin: auto; padding: 1rem;">
    {{ content }}
  </main>
</body>
</html>
EOF

# -----------------------------
# Convert markdown files
# -----------------------------
echo "Processing Markdown files..."

INDEX_LINKS=""

for file in "$SRC_DIR"/*.md; do
  [[ -e "$file" ]] || continue

  filename="$(basename "$file")"
  name="${filename%.md}"
  title="$(echo "$name" | sed 's/-/ /g; s/\b\(.\)/\u\1/g')"

  dest="$SITE_DIR/$filename"

  # Check if front matter exists
  if head -n 1 "$file" | grep -q '^---'; then
    cp "$file" "$dest"
  else
    cat > "$dest" <<EOF
---
layout: default
title: $title
---

$(cat "$file")
EOF
  fi

  INDEX_LINKS+="- [$title]($name.html)\n"
done

# -----------------------------
# Index page
# -----------------------------
cat > "$SITE_DIR/index.md" <<EOF
---
layout: default
title: Home
---

# Documentation

$INDEX_LINKS
EOF

echo "Site generated in $SITE_DIR"
echo "Next steps:"
echo "  cd $SITE_DIR"
echo "  bundle exec jekyll serve"