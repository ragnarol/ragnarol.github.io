#!/usr/bin/env bash
set -euo pipefail

SRC_DIR_ES="./source/es"
SRC_DIR_EN="./source/en"
SITE_DIR="./"

if [[ ! -d "$SRC_DIR_ES" ]] && [[ ! -d "$SRC_DIR_EN" ]]; then
  echo "Neither source directory exists: '$SRC_DIR_ES' or '$SRC_DIR_EN'"
  exit 1
fi

echo "Creating bilingual Jekyll site..."

mkdir -p "$SITE_DIR/_layouts"
mkdir -p "$SITE_DIR/assets"
mkdir -p "$SITE_DIR/es"
mkdir -p "$SITE_DIR/en"

# -------------------------------------------------
# Jekyll config
# -------------------------------------------------
cat > "$SITE_DIR/_config.yml" <<'EOF'
title: Piratas de Drinax
markdown: kramdown
collections:
  es:
    output: true
    permalink: /es/:name/
  en:
    output: true
    permalink: /en/:name/
EOF

# -------------------------------------------------
# GitHub-like CSS with language toggle
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

.lang-toggle {
  background: var(--sidebar-bg);
  border-bottom: 1px solid var(--border);
  padding: 0.75rem 1rem;
  display: flex;
  gap: 1rem;
  align-items: center;
}

.lang-toggle a {
  font-weight: 600;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  transition: background 0.2s;
}

.lang-toggle a:hover {
  background: rgba(0,0,0,0.05);
  text-decoration: none;
}

.lang-toggle a.active {
  background: var(--link);
  color: white;
}

.sidebar {
  width: 390px;
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
# Default layout with language-aware sidebar
# -------------------------------------------------
cat > "$SITE_DIR/_layouts/default.html" <<'EOF'
<!DOCTYPE html>
<html lang="{{ page.lang | default: 'es' }}">
<head>
  <meta charset="utf-8">
  <title>{{ page.title }} — {{ site.title }}</title>
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <link rel="stylesheet" href="{{ '/assets/style.css' | relative_url }}">
</head>
<body>
  <div class="lang-toggle">
    <span>Idioma:</span>
    <a href="{{ '/es/' | relative_url }}" {% if page.lang == 'es' or page.lang == nil %}class="active"{% endif %}>Español</a>
    <a href="{{ '/en/' | relative_url }}" {% if page.lang == 'en' %}class="active"{% endif %}>English</a>
  </div>

  <div class="layout">
    <nav class="sidebar">
      <h2>{{ site.title }}</h2>
      <ul>
        {% if page.lang == 'en' %}
          {% assign pages = site.pages | where: "lang", "en" %}
        {% elsif page.lang == 'es' %}
          {% assign pages = site.pages | where: "lang", "es" %}
        {% else %}
          {% assign pages = site.pages | where: "lang", "es" %}
        {% endif %}
        
        {% for p in pages %}
          {% if p.title %}
          <li>
            <a href="{{ p.url | relative_url }}">
              {{ p.title }}
            </a>
          </li>
          {% endif %}
        {% endfor %}
      </ul>
    </nav>

    <main class="content">
      <h1>
        {{ page.title | default: page.path | split: '/' | last | replace: '.md','' }}
      </h1>

      {{ content }}

      {% if page.comments != false %}
      <hr style="margin: 3rem 0;">

      <section id="comments">
        <script src="https://giscus.app/client.js"
          data-repo="ragnarol/ragnarol.github.io"
          data-repo-id="MDEwOlJlcG9zaXRvcnkxNzMxODM3Mjk="
          data-category="General"
          data-category-id="DIC_kwDOClKS8c4C1YPo"
          data-mapping="pathname"
          data-strict="0"
          data-reactions-enabled="1"
          data-emit-metadata="0"
          data-theme="light"
          crossorigin="anonymous"
          async>
        </script>
      </section>
      {% endif %}
    </main>
  </div>
</body>
</html>
EOF

# -------------------------------------------------
# Process Spanish Markdown files
# -------------------------------------------------
if [[ -d "$SRC_DIR_ES" ]]; then
  echo "Processing Spanish Markdown files..."
  
  for file in "$SRC_DIR_ES"/*.md; do
    [[ -e "$file" ]] || continue

    filename="$(basename "$file")"
    title="$(basename "$file" .md | sed 's/-/ /g; s/_/ /g; s/\b\(.\)/\u\1/g')"
    dest="$SITE_DIR/es/$filename"

    if head -n 1 "$file" | grep -q '^---'; then
      cp "$file" "$dest"
    else
      cat > "$dest" <<EOF
---
layout: default
title: $title
lang: es
---

$(cat "$file")
EOF
    fi
  done

  # Spanish index
  cat > "$SITE_DIR/es/index.md" <<'EOF'
---
layout: default
title: Inicio
lang: es
---

# Piratas de Drinax

Usa la barra de navegación en el lateral para explorar el contenido en español.
EOF
fi

# -------------------------------------------------
# Process English Markdown files
# -------------------------------------------------
if [[ -d "$SRC_DIR_EN" ]]; then
  echo "Processing English Markdown files..."
  
  for file in "$SRC_DIR_EN"/*.md; do
    [[ -e "$file" ]] || continue

    filename="$(basename "$file")"
    title="$(basename "$file" .md | sed 's/-/ /g; s/_/ /g; s/\b\(.\)/\u\1/g')"
    dest="$SITE_DIR/en/$filename"

    if head -n 1 "$file" | grep -q '^---'; then
      cp "$file" "$dest"
    else
      cat > "$dest" <<EOF
---
layout: default
title: $title
lang: en
---

$(cat "$file")
EOF
    fi
  done

  # English index
  cat > "$SITE_DIR/en/index.md" <<'EOF'
---
layout: default
title: Home
lang: en
---

# Pirates of Drinax

Use the sidebar navigation to explore the English content.
EOF
fi

# -------------------------------------------------
# Main index page (redirects to Spanish by default)
# -------------------------------------------------
cat > "$SITE_DIR/index.md" <<'EOF'
---
layout: default
title: Inicio
lang: es
---

# Piratas de Drinax / Pirates of Drinax

Selecciona tu idioma usando los botones en la parte superior:
- [Español](/es/)
- [English](/en/)
EOF

echo "✓ Site built successfully!"
echo "  Spanish files: $SITE_DIR/es/"
echo "  English files: $SITE_DIR/en/"