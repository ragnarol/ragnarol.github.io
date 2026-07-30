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
mkdir -p "$SITE_DIR/assets/js"
mkdir -p "$SITE_DIR/assets/themes"
mkdir -p "$SITE_DIR/_plugins"
mkdir -p "$SITE_DIR/es"
# Copy themes and plugins
if [[ -f "./assets/themes/sci-fi.css" ]]; then
  cp "./assets/themes/sci-fi.css" "$SITE_DIR/assets/themes/"
  cp "./assets/themes/gothic.css" "$SITE_DIR/assets/themes/"
fi
if [[ -f "./_plugins/search_index_generator.rb" ]]; then
  cp "./_plugins/search_index_generator.rb" "$SITE_DIR/_plugins/"
fi

mkdir -p "$SITE_DIR/en"

# -------------------------------------------------
# Jekyll config
# -------------------------------------------------
cat > "$SITE_DIR/_config.yml" <<'EOF'
title: RPG Campaign Summaries
markdown: kramdown
kramdown:
  hard_wrap: true
langs: [en, es]
plugins:
  - jekyll-relative-links
collections:
  es:
    output: true
    permalink: /es/:name/
  en:
    output: true
    permalink: /en/:name/
defaults:
  - scope:
      path: ""
    values:
      layout: default
  - scope:
      path: "es"
    values:
      lang: es
  - scope:
      path: "en"
    values:
      lang: en
  - scope:
      path: "es/pirates-of-drinax"
    values:
      theme: sci-fi
  - scope:
      path: "en/pirates-of-drinax"
    values:
      theme: sci-fi
  - scope:
      path: "es/ravenloft"
    values:
      theme: gothic
  - scope:
      path: "en/ravenloft"
    values:
      theme: gothic
EOF

# -------------------------------------------------
# Search styles
# -------------------------------------------------
cat > "$SITE_DIR/assets/js/search.js" <<'JSEOF'
document.addEventListener('DOMContentLoaded', function() {
  const searchInput = document.getElementById('search-input');
  const searchResults = document.getElementById('search-results');
  const searchNoResults = document.getElementById('search-no-results');

  if (!searchInput || !searchResults) return;

  let searchIndex = [];
  let currentIndex = -1;

  const currentLang = document.documentElement.lang || 'es';

  fetch(`/${currentLang}/search.json`)
    .then(response => response.json())
    .then(data => {
      searchIndex = data;
    })
    .catch(err => {
      console.error('Failed to load search index:', err);
    });

  function highlightText(text, query) {
    if (!query) return escapeHtml(text);
    const escaped = escapeHtml(text);
    const regex = new RegExp(`(${escapeRegex(query)})`, 'gi');
    return escaped.replace(regex, '<mark class="search-highlight">$1</mark>');
  }

  function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
  }

  function escapeRegex(string) {
    return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  }

  function performSearch(query) {
    if (!query || query.length < 2) {
      searchResults.innerHTML = '';
      searchResults.style.display = 'none';
      searchNoResults.style.display = 'none';
      return;
    }

    const lowerQuery = query.toLowerCase();
    const results = searchIndex.filter(page => {
      const titleLower = page.title.toLowerCase();
      const contentLower = page.content.toLowerCase();
      return titleLower.includes(lowerQuery) || contentLower.includes(lowerQuery);
    });

    renderResults(results, query);
  }

  function renderResults(results, query) {
    searchResults.innerHTML = '';

    if (results.length === 0) {
      searchResults.style.display = 'none';
      searchNoResults.style.display = 'block';
      searchNoResults.textContent = `No results found for "${escapeHtml(query)}"`;
      return;
    }

    searchNoResults.style.display = 'none';

    const limitedResults = results.slice(0, 10);

    const grouped = {};
    limitedResults.forEach(page => {
      if (!grouped[page.campaign]) {
        grouped[page.campaign] = [];
      }
      grouped[page.campaign].push(page);
    });

    for (const campaign in grouped) {
      const campaignDiv = document.createElement('div');
      campaignDiv.className = 'search-campaign';

      const campaignTitle = document.createElement('div');
      campaignTitle.className = 'search-campaign-title';
      campaignTitle.textContent = campaign.replace(/-/g, ' ').replace(/\b\w/g, c => c.toUpperCase());
      campaignDiv.appendChild(campaignTitle);

      const ul = document.createElement('ul');
      grouped[campaign].forEach(page => {
        const li = document.createElement('li');

        const link = document.createElement('a');
        link.href = page.url;
        link.innerHTML = `<span class="search-title">${highlightText(page.title, query)}</span>`;

        if (page.snippet) {
          const snippet = document.createElement('div');
          snippet.className = 'search-snippet';
          snippet.innerHTML = '...' + highlightText(page.snippet, query) + '...';
          link.appendChild(snippet);
        }

        li.appendChild(link);
        ul.appendChild(li);
      });

      campaignDiv.appendChild(ul);
      searchResults.appendChild(campaignDiv);
    }

    searchResults.style.display = 'block';
  }

  let debounceTimer;
  searchInput.addEventListener('input', function() {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
      performSearch(this.value.trim());
    }, 200);
  });

  searchInput.addEventListener('keydown', function(e) {
    const items = searchResults.querySelectorAll('a');
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      currentIndex = Math.min(currentIndex + 1, items.length - 1);
      updateActiveItem(items);
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      currentIndex = Math.max(currentIndex - 1, 0);
      updateActiveItem(items);
    } else if (e.key === 'Escape') {
      searchResults.style.display = 'none';
      searchInput.blur();
    }
  });

  function updateActiveItem(items) {
    items.forEach((item, i) => {
      item.classList.toggle('search-active', i === currentIndex);
    });
    if (items[currentIndex]) {
      items[currentIndex].scrollIntoView({ block: 'nearest' });
    }
  }

  document.addEventListener('click', function(e) {
    if (!e.target.closest('.search-container')) {
      searchResults.style.display = 'none';
      searchNoResults.style.display = 'none';
    }
  });

  document.addEventListener('keydown', function(e) {
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
      e.preventDefault();
      searchInput.focus();
      searchInput.select();
    }
  });
});
JSEOF

# -------------------------------------------------
# Main CSS
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

/* Search Styles */
.search-container {
  position: relative;
  margin: 0.75rem 0;
}

#search-input {
  width: 100%;
  padding: 0.5rem 0.75rem 0.5rem 2rem;
  border: 1px solid var(--border);
  border-radius: 6px;
  font-size: 0.9rem;
  background: var(--bg);
  color: var(--text);
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s;
}

#search-input:focus {
  border-color: var(--link);
  box-shadow: 0 0 0 2px rgba(9, 105, 218, 0.1);
}

.search-container::before {
  content: '\1F50D';
  position: absolute;
  left: 0.5rem;
  top: 50%;
  transform: translateY(-50%);
  font-size: 0.85rem;
  pointer-events: none;
  opacity: 0.5;
}

#search-results {
  display: none;
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: var(--bg);
  border: 1px solid var(--border);
  border-radius: 6px;
  max-height: 400px;
  overflow-y: auto;
  z-index: 1000;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  margin-top: 0.25rem;
}

.search-campaign {
  padding: 0.25rem 0;
}

.search-campaign + .search-campaign {
  border-top: 1px solid var(--border);
}

.search-campaign-title {
  padding: 0.35rem 0.75rem;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--text);
  opacity: 0.6;
  background: var(--sidebar-bg);
  border-radius: 4px 4px 0 0;
}

#search-results ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

#search-results li {
  margin: 0;
}

#search-results a {
  display: block;
  padding: 0.5rem 0.75rem;
  text-decoration: none;
  color: var(--link);
  border-left: none;
}

#search-results a:hover,
#search-results a.search-active {
  background: var(--sidebar-bg);
  color: var(--link);
}

.search-title {
  font-weight: 600;
  display: block;
}

.search-snippet {
  font-size: 0.8rem;
  color: var(--text);
  opacity: 0.7;
  margin-top: 0.15rem;
  display: block;
  line-height: 1.3;
}

.search-no-results {
  padding: 0.75rem;
  font-size: 0.85rem;
  color: var(--text);
  opacity: 0.6;
  text-align: center;
  display: none;
}

.search-highlight {
  background: rgba(255, 230, 0, 0.3);
  border-radius: 2px;
  padding: 0 1px;
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
  {% if page.theme %}
  <link rel="stylesheet" href="{{ '/assets/themes/' | append: page.theme | append: '.css' | relative_url }}">
  {% endif %}
  <script src="{{ '/assets/js/search.js' | relative_url }}"></script>
</head>
<body>
  <div class="lang-toggle">
    <span>Idioma:</span>
    <a href="{{ '/es/' | relative_url }}" {% if page.lang == 'es' or page.lang == nil %}class="active"{% endif %}>Español</a>
    <a href="{{ '/en/' | relative_url }}" {% if page.lang == 'en' %}class="active"{% endif %}>English</a>
  </div>

  <div class="layout">
    <nav class="sidebar">
      <h2><a href="{{ '/' | relative_url }}">{{ site.title }}</a></h2>

      <div class="search-container">
        <input type="text" id="search-input" placeholder="Search pages..." autocomplete="off">
        <div id="search-results"></div>
        <div id="search-no-results" class="search-no-results"></div>
      </div>

      {% assign current_lang = page.lang | default: 'es' %}
      {% assign lang_pages = site.pages | where: "lang", current_lang | sort: "path" %}

      {% assign current_path_parts = page.path | split: "/" %}
      {% assign current_campaign = "" %}
      {% if current_path_parts.size > 2 %}
        {% assign current_campaign = current_path_parts[1] %}
      {% endif %}

      {% assign campaign_list = "" | split: "" %}
      {% for p in lang_pages %}
        {% assign p_parts = p.path | split: "/" %}
        {% if p_parts.size > 2 %}
          {% assign c_id = p_parts[1] %}
          {% unless campaign_list contains c_id %}
            {% assign campaign_list = campaign_list | push: c_id %}
          {% endunless %}
        {% endif %}
      {% endfor %}

      {% for c_id in campaign_list %}
        {% assign is_current = false %}
        {% if c_id == current_campaign %}
          {% assign is_current = true %}
        {% endif %}

        <details {% if is_current %}open{% endif %}>
          <summary><strong>{{ c_id | replace: "-", " " | capitalize }}</strong></summary>
          <ul>
            {% for p in lang_pages %}
              {% assign p_parts = p.path | split: "/" %}
              {% if p_parts[1] == c_id %}
                {% if p.name != "index.md" %}
                <li>
                  <a href="{{ p.url | relative_url }}" {% if p.url == page.url %}class="active-link"{% endif %}>
                    {{ p.name | replace: '.md','', replace: ' - eng', '' | replace: ' - summary', '' | replace: '- eng', '' | replace: '- summary', '' | replace: '-', ' ' | replace: '_', ' ' | replace: '  ', ' ' }}
                  </a>
                </li>
                {% endif %}
              {% endif %}
            {% endfor %}
          </ul>
        </details>
      {% endfor %}
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

# -------------------------------------------------
# Generate search index JSON files for client-side search
# -------------------------------------------------
generate_search_index() {
  local lang=$1
  local src_dir=$2
  local out_dir=$SITE_DIR/$lang
  local first=true

  echo "[{" > "$out_dir/search.json.tmp"

  for file in "$src_dir"/**/*.md; do
    [[ -e "$file" ]] || continue
    
    # Skip index.md files
    local basename
    basename=$(basename "$file")
    [[ "$basename" == "index.md" ]] && continue

    # Extract title from frontmatter or filename
    local title
    if head -n 5 "$file" | grep -q '^title:'; then
      title=$(head -n 5 "$file" | grep '^title:' | sed 's/^title: *//' | sed 's/^"//;s/"$//')
    else
      title=$(basename "$file" .md | sed 's/-/ /g; s/_/ /g; s/\b\(.*\)/\u\1/g')
    fi

    # Extract campaign from path
    local campaign
    campaign=$(echo "$file" | sed "s|$src_dir/||" | cut -d'/' -f1)

    # Clean content: strip markdown, join lines
    local content
    content=$(cat "$file" | \
      sed '/^---$/d' | \
      sed 's/^#{1,6}[[:space:]]*//' | \
      sed 's/^\*\*\(.*\)\*\*/\1/g' | \
      sed 's/^\*\(.*\)\*/\1/g' | \
      sed 's/^---*$//' | \
      sed 's/^[-*+][[:space:]]//' | \
      tr '\n' ' ' | \
      sed 's/  */ /g; s/^ *//; s/ *$//')

    # Escape JSON strings
    title=$(echo "$title" | sed 's/\\/\\\\/g; s/"/\\"/g')
    content=$(echo "$content" | sed 's/\\/\\\\/g; s/"/\\"/g')

    if [ "$first" = true ]; then
      first=false
    else
      echo "," >> "$out_dir/search.json.tmp"
    fi

    # Add to index
    cat >> "$out_dir/search.json.tmp" <<JSONEOF
  {
    "title": "$title",
    "url": "/$lang/$campaign/$basename",
    "content": "$content",
    "campaign": "$campaign"
  }
JSONEOF
  done

  echo "}]" >> "$out_dir/search.json.tmp"
  mv "$out_dir/search.json.tmp" "$out_dir/search.json"
}

if [[ -d "$SRC_DIR_ES" ]]; then
  generate_search_index "es" "$SRC_DIR_ES"
  echo "✓ Generated Spanish search index"
fi

if [[ -d "$SRC_DIR_EN" ]]; then
  generate_search_index "en" "$SRC_DIR_EN"
  echo "✓ Generated English search index"
fi

echo "✓ Site built successfully!"
echo "  Spanish files: $SITE_DIR/es/"
echo "  English files: $SITE_DIR/en/"