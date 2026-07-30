module Jekyll
  class SearchIndexGenerator < Generator
    safe true

    def generate(site)
      # Determine languages from directories
      languages = []
      site.config['langs'] ||= []
      site.config['langs'].each { |l| languages << l }

      # Auto-detect language directories
      if File.exist?(File.join(site.source, 'en'))
        languages << 'en' unless languages.include?('en')
      end
      if File.exist?(File.join(site.source, 'es'))
        languages << 'es' unless languages.include?('es')
      end

      languages.uniq!

      languages.each do |lang|
        generate_search_index(site, lang)
      end
    end

    private

    def generate_search_index(site, lang)
      pages = []

      site.pages.each do |page|
        # Only include pages in the correct language directory
        next unless page.path.start_with?("#{lang}/")
        next if File.basename(page.path) == 'index.md'
        next unless File.extname(page.path) == '.md'

        # Clean up the page title
        title = File.basename(page.path, '.md')
        title = clean_title(title)

        # Read and clean the content
        content = site.read_content(page.path)
        content = strip_html_and_symbols(content)

        # Extract campaign name from path
        path_parts = page.path.split('/')
        campaign = path_parts.length > 2 ? path_parts[1] : 'general'

        pages << {
          'title' => title,
          'url' => page.url,
          'content' => content,
          'campaign' => campaign
        }
      end

      # Sort pages by path for consistent ordering
      pages.sort_by! { |p| p['url'] }

      # Write the search index JSON
      output_dir = File.join(site.dest, lang)
      FileUtils.mkdir_p(output_dir)
      File.write(File.join(output_dir, 'search.json'), JSON.pretty_generate(pages))

      Jekyll.logger.info "Search Index:", "Generated search index for '#{lang}' with #{pages.length} pages"
    end

    def clean_title(title)
      # Remove common suffixes and clean up
      title = title.gsub(/ - eng$/, '')
      title = title.gsub(/ - summary$/, '')
      title = title.gsub(/- summary$/, '')
      title = title.gsub(/\s+-\s+/, ' - ')
      title = title.gsub(/_/, ' ')
      title = title.gsub(/\s{2,}/, ' ').strip
      title
    end

    def strip_html_and_symbols(content)
      # Strip markdown formatting symbols but keep text content
      content = content.gsub(/#{4,}/, '')     # code blocks
      content = content.gsub(/`/, '')          # inline code
      content = content.gsub(/\*\*(.*?)\*\*/, '\1')  # bold
      content = content.gsub(/\*(.*?)\*/, '\1')    # italic
      content = content.gsub(/---+/, '')        # horizontal rules
      content = content.gsub(/#{1,6}\s+/, '')   # headers
      content = content.gsub(/^\s*[-*+]\s+/, '') # list items
      content = content.gsub(/\n+/, ' ')        # newlines to spaces
      content.strip
    end
  end
end
