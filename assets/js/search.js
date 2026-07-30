document.addEventListener('DOMContentLoaded', function() {
  const searchInput = document.getElementById('search-input');
  const searchResults = document.getElementById('search-results');
  const searchNoResults = document.getElementById('search-no-results');

  if (!searchInput || !searchResults) return;

  let searchIndex = [];
  let currentIndex = -1;

  // Determine language from page
  const currentLang = document.documentElement.lang || 'es';

  // Fetch the search index for the current language
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

    // Limit to top 10 results
    const limitedResults = results.slice(0, 10);

    // Group by campaign
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

        // Show context snippet if query matches content
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

  // Keyboard navigation
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

  // Close search results when clicking outside
  document.addEventListener('click', function(e) {
    if (!e.target.closest('.search-container')) {
      searchResults.style.display = 'none';
      searchNoResults.style.display = 'none';
    }
  });

  // Focus search on Ctrl+K or Cmd+K
  document.addEventListener('keydown', function(e) {
    if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
      e.preventDefault();
      searchInput.focus();
      searchInput.select();
    }
  });
});
