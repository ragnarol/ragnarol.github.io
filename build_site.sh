#!/usr/bin/env bash
set -euo pipefail

SRC_DIR="./source"
SITE_DIR="./"

if [[ ! -d "$SRC_DIR" ]]; then
  echo "Source directory '$SRC_DIR' does not exist"
  exit 1
fi

echo "Creating Jekyll site with sidebar..."

mkdir -p "$SITE_DIR/_layouts"
mkdir -p "$SITE_DIR/assets"

# -------------------------------------------------
# Jekyll config
# -------------------------------------------------
cat > "$SITE_DIR/_config.yml" <<'EOF'
title: Piratas de Drinax
markdown: kramdown
EOF

# -------------------------------------------------
# GitHub-like CSS
# -------------------------------------------------
cat > "$SITE_DIR/assets/style.css" <<'EOF'
:root {
  --bg: #ffffff;
  --sidebar-bg: #f6f8fa;
  --border: #d0d7de;
  --text: #24292f;
  --link: #0969da;
  --code-bg: #f6f8fa;
}

* {
  box-sizing: border-box;
}

body {
  margin: 0;
  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI",
               Helvetica, Arial, sans-serif;
  color: var(--text);
  background: var(--bg);
}

a {
  color: var(--link);
  text-decoration: none;
}
a:hover {
  text-decoration: underline;
}

.layout {
  display: flex;
  min-height: 100vh;
}

.sidebar {
  width: 260px;
  background: var(--sidebar-bg);
  border-right: 1px solid var(--border);
  padding: 1rem;
}

.sidebar h2 {
  font-size: 1rem;
  margin-top: 0;
}

.sidebar ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.sidebar li {
  margin: 0.4rem 0;
}

.content {
  flex: 1;
  padding: 2rem;
  max-width: 900px;
}

pre, code {
  background: var(--code-bg);
  border-radius: 6px;
}

pre {
  padding: 1rem;
  overflow-x: auto;
}

code {
  padding: 0.2em 0.4em;
}

h1, h2, h3 {
  border-bottom: 1px solid var(--border);
  padding-bottom: 0.3em;
}

@media (max-width: 800px) {
  .layout {
    flex-direction: column;
  }
  .sidebar {
    width: 100%;
    border-right: none;
    border-bottom: 1px solid var(--border);
  }
}
EOF

# -------------------------------------------------
# Default layout with sidebar
# -------------------------------------------------
cat > "$SITE_DIR/_layouts/default.html" <<'EOF'
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <title>{{ page.title }} – {{ site.title }}</title>
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <link rel="stylesheet" href="{{ '/assets/style.css' | relative_url }}">
</head>
<body>
  <div class="layout">
    <nav class="sidebar">
      <h2>{{ site.title }}</h2>
      <ul>
        {% for page in site.pages %}
          {% if page.title %}
          <li>
            <a href="{{ page.url | relative_url }}">
              {{ page.title }}
            </a>
          </li>
          {% endif %}
        {% endfor %}
      </ul>
    </nav>
    <main class="content">
      {{ content }}
    </main>
  </div>
</body>
</html>
EOF

# -------------------------------------------------
# Convert Markdown files
# -------------------------------------------------
echo "Processing Markdown files..."

for file in "$SRC_DIR"/*.md; do
  [[ -e "$file" ]] || continue

  filename="$(basename "$file")"
  name="${filename%.md}"
  title="$(basename "$file" .md | sed 's/-/ /g; s/_/ /g; s/\b\(.\)/\u\1/g')"

  dest="$SITE_DIR/$filename"

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
done

# -------------------------------------------------
# Index page
# -------------------------------------------------
cat > "$SITE_DIR/index.md" <<'EOF'
---
layout: default
title: Home
---

# Piratas de Drinax

Usa la barra de navegacion en el lateral
EOF
