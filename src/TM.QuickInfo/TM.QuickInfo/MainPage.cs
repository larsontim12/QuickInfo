using System;
using System.Linq;
using Wisej.Web;
using QuickInfo;

namespace TM.QuickInfo
{
    public partial class MainPage : Page
    {
        private Engine engine;

        public MainPage()
        {
            InitializeComponent();
            InitializeEngine();
            txtSearch.Focus();
        }

        private void InitializeEngine()
        {
            // Initialize QuickInfo Engine with all processors from both assemblies
            engine = new Engine(
                typeof(Engine).Assembly,
                typeof(Processors.Ip).Assembly
            );
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            PerformSearch();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                PerformSearch();
                e.Handled = true;
            }
        }

        private void PerformSearch()
        {
            string queryText = txtSearch.Text?.Trim();

            if (string.IsNullOrEmpty(queryText))
            {
                return;
            }

            try
            {
                // Create query
                var query = new WebQuery(queryText);

                // Get results from engine
                var results = engine.GetResults(query);

                if (results == null || !results.Any())
                {
                    ShowNoResults();
                    return;
                }

                // Render to HTML view
                RenderHtmlResults(results);

                // Render to native Wisej view
                RenderNativeResults(results);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void RenderHtmlResults(System.Collections.Generic.IEnumerable<(string processorName, object resultNode)> results)
        {
            try
            {
                string html = HtmlRenderer.RenderObject(results);

                // Wrap in basic HTML with styling
                string fullHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; padding: 10px; background-color: white; }}
        .answersList {{ }}
        .answerBlock {{ margin: 10px 0; padding: 10px; border: 1px solid #ddd; border-radius: 5px; }}
        .answerBlockHeader {{ font-weight: bold; font-size: 14pt; color: #0066cc; margin-bottom: 10px; }}
        .singleAnswerSection {{ padding: 5px; }}
        .mainAnswerText {{ font-size: 12pt; }}
        .sectionHeader {{ font-weight: bold; color: #444; margin-top: 10px; }}
        .fixed {{ font-family: 'Courier New', monospace; }}
        .gray {{ color: #666; }}
        .note {{ color: #666; font-style: italic; }}
        .charSample {{ font-size: 20pt; }}
        .swatch {{ width: 100px; height: 50px; display: inline-block; border: 1px solid #333; margin: 5px; }}
        .swatchName {{ font-size: 10pt; margin: 5px; }}
        .inlineBlock {{ display: inline-block; margin: 5px; }}
        table {{ border-collapse: collapse; margin: 10px 0; }}
        td, th {{ padding: 5px 10px; border: 1px solid #ddd; }}
        th {{ background-color: #f0f0f0; font-weight: bold; }}
        a {{ color: #0066cc; text-decoration: none; }}
        a:hover {{ text-decoration: underline; }}
    </style>
</head>
<body>
{html}
</body>
</html>";

                webBrowser.Html = fullHtml;
            }
            catch (Exception ex)
            {
                webBrowser.Html = $"<html><body><div style='color: red;'>Error rendering HTML: {System.Net.WebUtility.HtmlEncode(ex.Message)}</div></body></html>";
            }
        }

        private void RenderNativeResults(System.Collections.Generic.IEnumerable<(string processorName, object resultNode)> results)
        {
            try
            {
                // Clear previous results
                pnlNativeResults.Controls.Clear();

                var containerPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    Padding = new Padding(10)
                };

                int yPosition = 10;
                int availableWidth = pnlNativeResults.ClientSize.Width - 40;

                if (results.Count() == 1)
                {
                    // Single result - render without processor name
                    var result = results.First();
                    var contentPanel = new Panel
                    {
                        Location = new System.Drawing.Point(10, yPosition),
                        Width = availableWidth,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink
                    };

                    RenderResultObjectAt(result.resultNode, contentPanel, 0, availableWidth);
                    containerPanel.Controls.Add(contentPanel);
                }
                else
                {
                    // Multiple results - show processor names
                    foreach (var result in results)
                    {
                        var resultPanel = new Panel
                        {
                            Location = new System.Drawing.Point(10, yPosition),
                            Width = availableWidth,
                            AutoSize = true,
                            AutoSizeMode = AutoSizeMode.GrowAndShrink,
                            BorderStyle = BorderStyle.Solid,
                            Padding = new Padding(10)
                        };

                        int innerY = 5;

                        // Add processor name header
                        var headerLabel = new Label
                        {
                            Text = result.processorName,
                            Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold),
                            ForeColor = System.Drawing.Color.FromArgb(0, 102, 204),
                            AutoSize = true,
                            Location = new System.Drawing.Point(5, innerY)
                        };
                        resultPanel.Controls.Add(headerLabel);
                        innerY += headerLabel.Height + 10;

                        // Render the result content
                        innerY = RenderResultObjectAt(result.resultNode, resultPanel, innerY, availableWidth - 30);

                        containerPanel.Controls.Add(resultPanel);
                        yPosition += resultPanel.Height + 15;
                    }
                }

                pnlNativeResults.Controls.Add(containerPanel);
            }
            catch (Exception ex)
            {
                var errorLabel = new Label
                {
                    Text = $"Error rendering native results: {ex.Message}\n\n{ex.StackTrace}",
                    ForeColor = System.Drawing.Color.Red,
                    AutoSize = true,
                    Location = new System.Drawing.Point(10, 10),
                    MaximumSize = new System.Drawing.Size(pnlNativeResults.Width - 20, 0)
                };
                pnlNativeResults.Controls.Clear();
                pnlNativeResults.Controls.Add(errorLabel);
            }
        }

        private int RenderResultObjectAt(object result, Control container, int yPosition, int maxWidth)
        {
            switch (result)
            {
                case string s:
                    var label = new Label
                    {
                        Text = s,
                        AutoSize = true,
                        Font = new System.Drawing.Font("Segoe UI", 11),
                        MaximumSize = new System.Drawing.Size(maxWidth, 0),
                        Location = new System.Drawing.Point(5, yPosition)
                    };
                    container.Controls.Add(label);
                    return yPosition + label.Height + 5;

                case Node node:
                    return RenderNodeAt(node, container, yPosition, maxWidth);

                case System.Collections.Generic.IEnumerable<object> list:
                    foreach (var item in list)
                    {
                        yPosition = RenderResultObjectAt(item, container, yPosition, maxWidth);
                    }
                    return yPosition;

                default:
                    if (result != null)
                    {
                        var defaultLabel = new Label
                        {
                            Text = result.ToString(),
                            AutoSize = true,
                            MaximumSize = new System.Drawing.Size(maxWidth, 0),
                            Location = new System.Drawing.Point(5, yPosition)
                        };
                        container.Controls.Add(defaultLabel);
                        return yPosition + defaultLabel.Height + 5;
                    }
                    return yPosition;
            }
        }

        private int RenderNodeAt(Node node, Control container, int yPosition, int maxWidth)
        {
            // Handle tables
            if (node.Kind == NodeKinds.Table)
            {
                return RenderTableAt(node, container, yPosition, maxWidth);
            }

            // Handle color swatches
            if (node.Style == NodeStyles.ColorSwatchLarge || node.Style == NodeStyles.ColorSwatchSmall)
            {
                var colorPanel = new Panel
                {
                    BackColor = ParseColor(node.Text),
                    Size = node.Style == NodeStyles.ColorSwatchLarge
                        ? new System.Drawing.Size(System.Math.Min(300, maxWidth), 50)
                        : new System.Drawing.Size(60, 16),
                    BorderStyle = BorderStyle.Solid,
                    Location = new System.Drawing.Point(5, yPosition)
                };
                container.Controls.Add(colorPanel);
                return yPosition + colorPanel.Height + 10;
            }

            // Handle lists
            if (node.List != null && node.List.Any())
            {
                if (node.Style == NodeStyles.Card)
                {
                    // Card style - create bordered panel
                    var cardPanel = new Panel
                    {
                        Location = new System.Drawing.Point(5, yPosition),
                        Width = maxWidth - 10,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        BorderStyle = BorderStyle.Solid,
                        Padding = new Padding(10)
                    };

                    int cardY = 5;

                    if (node.Text != null)
                    {
                        var headerLabel = new Label
                        {
                            Text = node.Text,
                            Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold),
                            AutoSize = true,
                            Location = new System.Drawing.Point(5, cardY)
                        };
                        cardPanel.Controls.Add(headerLabel);
                        cardY += headerLabel.Height + 10;
                    }

                    foreach (var item in node.List)
                    {
                        cardY = RenderResultObjectAt(item, cardPanel, cardY, maxWidth - 40);
                    }

                    container.Controls.Add(cardPanel);
                    return yPosition + cardPanel.Height + 10;
                }
                else if (node.Style == NodeStyles.HorizontalList)
                {
                    // Horizontal list - render items side by side
                    int xPos = 5;
                    int maxHeight = 0;
                    foreach (var item in node.List)
                    {
                        if (item is Node itemNode && itemNode.Text != null)
                        {
                            var itemLabel = new Label
                            {
                                Text = itemNode.Text,
                                AutoSize = true,
                                Location = new System.Drawing.Point(xPos, yPosition)
                            };
                            container.Controls.Add(itemLabel);
                            xPos += itemLabel.Width + 10;
                            maxHeight = System.Math.Max(maxHeight, itemLabel.Height);
                        }
                    }
                    return yPosition + maxHeight + 10;
                }
                else
                {
                    // Vertical list
                    foreach (var item in node.List)
                    {
                        yPosition = RenderResultObjectAt(item, container, yPosition, maxWidth);
                    }
                    return yPosition;
                }
            }
            else if (node.Text != null)
            {
                // Simple text node
                var label = new Label
                {
                    Text = node.Text,
                    AutoSize = true,
                    MaximumSize = new System.Drawing.Size(maxWidth, 0),
                    Location = new System.Drawing.Point(5, yPosition)
                };

                // Apply styling
                if (node.Style == NodeStyles.Fixed)
                {
                    label.Font = new System.Drawing.Font("Courier New", 10);
                }
                else if (node.Style == NodeStyles.SectionHeader)
                {
                    label.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                    label.ForeColor = System.Drawing.Color.FromArgb(68, 68, 68);
                }
                else if (node.Style == NodeStyles.Label)
                {
                    label.ForeColor = System.Drawing.Color.FromArgb(102, 102, 102);
                }
                else if (node.Style == NodeStyles.CharSample)
                {
                    label.Font = new System.Drawing.Font("Segoe UI", 16);
                }

                container.Controls.Add(label);
                return yPosition + label.Height + 5;
            }

            return yPosition;
        }

        private int RenderTableAt(Node tableNode, Control container, int yPosition, int maxWidth)
        {
            if (tableNode.List == null || !tableNode.List.Any())
            {
                return yPosition;
            }

            // Check if this is a color table (cells with Color style)
            bool isColorTable = tableNode.Style == NodeStyles.Color ||
                               tableNode.List.Cast<Node>().Any(row =>
                                   row.List?.Cast<Node>().Any(cell => cell.Style == NodeStyles.Color) ?? false);

            if (isColorTable)
            {
                return RenderColorGrid(tableNode, container, yPosition, maxWidth);
            }

            // Regular table - use DataGridView
            var gridView = new DataGridView
            {
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                BorderStyle = BorderStyle.Solid,
                Location = new System.Drawing.Point(5, yPosition),
                Width = System.Math.Min(maxWidth - 10, 800),
                Height = 200
            };

            // Check if first row is headers
            var firstRow = tableNode.List.FirstOrDefault() as Node;
            bool hasHeaders = firstRow?.List?.Any(cell => (cell as Node)?.Kind == NodeKinds.ColumnHeader) ?? false;

            // Determine column count
            int colCount = 0;
            foreach (var row in tableNode.List.Cast<Node>())
            {
                if (row.List != null)
                {
                    colCount = System.Math.Max(colCount, row.List.Count());
                }
            }

            // Add columns
            for (int i = 0; i < colCount; i++)
            {
                gridView.Columns.Add($"col{i}", hasHeaders ? "" : $"Column {i + 1}");
            }

            // Add rows
            if (hasHeaders && firstRow?.List != null)
            {
                // Set column headers
                int colIdx = 0;
                foreach (var cell in firstRow.List.Cast<Node>())
                {
                    if (colIdx < gridView.Columns.Count)
                    {
                        gridView.Columns[colIdx].HeaderText = cell.Text ?? "";
                    }
                    colIdx++;
                }
            }

            // Add data rows
            foreach (var row in tableNode.List.Skip(hasHeaders ? 1 : 0).Cast<Node>())
            {
                if (row.List != null)
                {
                    var values = new object[colCount];
                    int colIdx = 0;
                    foreach (var cell in row.List.Cast<Node>())
                    {
                        if (colIdx < colCount)
                        {
                            values[colIdx] = cell.Text ?? "";
                        }
                        colIdx++;
                    }
                    gridView.Rows.Add(values);
                }
            }

            container.Controls.Add(gridView);
            return yPosition + gridView.Height + 15;
        }

        private int RenderColorGrid(Node tableNode, Control container, int yPosition, int maxWidth)
        {
            const int cellWidth = 100;
            const int cellHeight = 70;
            const int cellSpacing = 10;
            const int labelHeight = 18;

            int xPos = 5;
            int currentY = yPosition;
            int maxRowHeight = 0;
            int colsPerRow = (maxWidth - 10) / (cellWidth + cellSpacing);
            int currentCol = 0;

            foreach (var row in tableNode.List.Cast<Node>())
            {
                if (row.List == null) continue;

                foreach (var cell in row.List.Cast<Node>())
                {
                    if (currentCol >= colsPerRow)
                    {
                        // Move to next row
                        currentY += maxRowHeight + cellSpacing;
                        xPos = 5;
                        currentCol = 0;
                        maxRowHeight = 0;
                    }

                    // Create color swatch panel
                    var colorPanel = new Panel
                    {
                        BackColor = ParseColor(cell.Text),
                        Size = new System.Drawing.Size(cellWidth, cellHeight - labelHeight),
                        BorderStyle = BorderStyle.Solid,
                        Location = new System.Drawing.Point(xPos, currentY)
                    };
                    container.Controls.Add(colorPanel);

                    // Create color name label below the swatch
                    var nameLabel = new Label
                    {
                        Text = cell.Text,
                        AutoSize = false,
                        Size = new System.Drawing.Size(cellWidth, labelHeight),
                        Location = new System.Drawing.Point(xPos, currentY + cellHeight - labelHeight),
                        TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                        Font = new System.Drawing.Font("Segoe UI", 8)
                    };
                    container.Controls.Add(nameLabel);

                    xPos += cellWidth + cellSpacing;
                    currentCol++;
                    maxRowHeight = System.Math.Max(maxRowHeight, cellHeight);
                }
            }

            return currentY + maxRowHeight + cellSpacing;
        }

        private void RenderResultObject(object result, Control container)
        {
            switch (result)
            {
                case string s:
                    var label = new Label
                    {
                        Text = s,
                        AutoSize = true,
                        Font = new System.Drawing.Font("Segoe UI", 11),
                        MaximumSize = new System.Drawing.Size(container.Width - 20, 0),
                        Margin = new Padding(0, 2, 0, 2)
                    };
                    container.Controls.Add(label);
                    break;

                case Node node:
                    RenderNode(node, container);
                    break;

                case System.Collections.Generic.IEnumerable<object> list:
                    foreach (var item in list)
                    {
                        RenderResultObject(item, container);
                    }
                    break;

                default:
                    if (result != null)
                    {
                        var defaultLabel = new Label
                        {
                            Text = result.ToString(),
                            AutoSize = true,
                            MaximumSize = new System.Drawing.Size(container.Width - 20, 0),
                            Margin = new Padding(0, 2, 0, 2)
                        };
                        container.Controls.Add(defaultLabel);
                    }
                    break;
            }
        }

        private void RenderNode(Node node, Control container)
        {
            // Handle tables
            if (node.Kind == NodeKinds.Table)
            {
                RenderTable(node, container);
                return;
            }

            // Handle color swatches
            if (node.Style == NodeStyles.ColorSwatchLarge || node.Style == NodeStyles.ColorSwatchSmall)
            {
                var colorPanel = new Panel
                {
                    BackColor = ParseColor(node.Text),
                    Size = node.Style == NodeStyles.ColorSwatchLarge
                        ? new System.Drawing.Size(300, 50)
                        : new System.Drawing.Size(60, 16),
                    BorderStyle = BorderStyle.Solid,
                    Margin = new Padding(0, 5, 0, 5)
                };
                container.Controls.Add(colorPanel);
                return;
            }

            // Handle lists
            if (node.List != null && node.List.Any())
            {
                if (node.Style == NodeStyles.Card)
                {
                    // Card style - create bordered panel
                    var cardPanel = new Panel
                    {
                        AutoSize = true,
                        BorderStyle = BorderStyle.Solid,
                        Padding = new Padding(10),
                        Margin = new Padding(0, 5, 0, 5),
                        Width = container.Width - 20
                    };

                    if (node.Text != null)
                    {
                        var headerLabel = new Label
                        {
                            Text = node.Text,
                            Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold),
                            AutoSize = true,
                            Margin = new Padding(0, 0, 0, 5)
                        };
                        cardPanel.Controls.Add(headerLabel);
                    }

                    var contentFlow = new FlowLayoutPanel
                    {
                        FlowDirection = FlowDirection.TopDown,
                        AutoSize = true,
                        Dock = DockStyle.Fill,
                        WrapContents = false
                    };

                    foreach (var item in node.List)
                    {
                        RenderResultObject(item, contentFlow);
                    }

                    cardPanel.Controls.Add(contentFlow);
                    container.Controls.Add(cardPanel);
                }
                else if (node.Style == NodeStyles.HorizontalList)
                {
                    // Horizontal list
                    var hPanel = new FlowLayoutPanel
                    {
                        FlowDirection = FlowDirection.LeftToRight,
                        AutoSize = true,
                        WrapContents = true,
                        Margin = new Padding(0, 5, 0, 5)
                    };

                    foreach (var item in node.List)
                    {
                        RenderResultObject(item, hPanel);
                    }

                    container.Controls.Add(hPanel);
                }
                else
                {
                    // Vertical list
                    foreach (var item in node.List)
                    {
                        RenderResultObject(item, container);
                    }
                }
            }
            else if (node.Text != null)
            {
                // Simple text node
                var label = new Label
                {
                    Text = node.Text,
                    AutoSize = true,
                    MaximumSize = new System.Drawing.Size(container.Width - 20, 0),
                    Margin = new Padding(0, 2, 0, 2)
                };

                // Apply styling
                if (node.Style == NodeStyles.Fixed)
                {
                    label.Font = new System.Drawing.Font("Courier New", 10);
                }
                else if (node.Style == NodeStyles.SectionHeader)
                {
                    label.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                    label.ForeColor = System.Drawing.Color.FromArgb(68, 68, 68);
                }
                else if (node.Style == NodeStyles.Label)
                {
                    label.ForeColor = System.Drawing.Color.FromArgb(102, 102, 102);
                }
                else if (node.Style == NodeStyles.CharSample)
                {
                    label.Font = new System.Drawing.Font("Segoe UI", 16);
                }

                container.Controls.Add(label);
            }
        }

        private void RenderTable(Node tableNode, Control container)
        {
            var gridView = new DataGridView
            {
                AutoSize = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                BorderStyle = BorderStyle.Solid,
                Margin = new Padding(0, 5, 0, 5),
                MaximumSize = new System.Drawing.Size(container.Width - 20, 600)
            };

            if (tableNode.List == null || !tableNode.List.Any())
            {
                return;
            }

            // Check if first row is headers
            var firstRow = tableNode.List.FirstOrDefault() as Node;
            bool hasHeaders = firstRow?.List?.Any(cell => (cell as Node)?.Kind == NodeKinds.ColumnHeader) ?? false;

            // Determine column count
            int colCount = 0;
            foreach (var row in tableNode.List.Cast<Node>())
            {
                if (row.List != null)
                {
                    colCount = System.Math.Max(colCount, row.List.Count());
                }
            }

            // Add columns
            for (int i = 0; i < colCount; i++)
            {
                gridView.Columns.Add($"col{i}", hasHeaders ? "" : $"Column {i + 1}");
            }

            // Add rows
            int startRow = hasHeaders ? 1 : 0;
            if (hasHeaders && firstRow?.List != null)
            {
                // Set column headers
                int colIdx = 0;
                foreach (var cell in firstRow.List.Cast<Node>())
                {
                    if (colIdx < gridView.Columns.Count)
                    {
                        gridView.Columns[colIdx].HeaderText = cell.Text ?? "";
                    }
                    colIdx++;
                }
            }

            // Add data rows
            foreach (var row in tableNode.List.Skip(hasHeaders ? 1 : 0).Cast<Node>())
            {
                if (row.List != null)
                {
                    var values = new object[colCount];
                    int colIdx = 0;
                    foreach (var cell in row.List.Cast<Node>())
                    {
                        if (colIdx < colCount)
                        {
                            values[colIdx] = cell.Text ?? "";
                        }
                        colIdx++;
                    }
                    gridView.Rows.Add(values);
                }
            }

            container.Controls.Add(gridView);
        }

        private System.Drawing.Color ParseColor(string colorString)
        {
            try
            {
                if (string.IsNullOrEmpty(colorString))
                    return System.Drawing.Color.White;

                // Handle hex colors
                if (colorString.StartsWith("#"))
                {
                    string hex = colorString.TrimStart('#');
                    if (hex.Length == 6)
                    {
                        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                        int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                        return System.Drawing.Color.FromArgb(r, g, b);
                    }
                }

                // Try to parse named color
                return System.Drawing.Color.FromName(colorString);
            }
            catch
            {
                return System.Drawing.Color.White;
            }
        }

        private void ShowNoResults()
        {
            webBrowser.Html = "<html><body><div style='color: #666; font-style: italic; padding: 20px;'>No results found. Enter ? for help.</div></body></html>";

            pnlNativeResults.Controls.Clear();
            var noResultsLabel = new Label
            {
                Text = "No results found. Enter ? for help.",
                ForeColor = System.Drawing.Color.Gray,
                AutoSize = true,
                Location = new System.Drawing.Point(10, 10)
            };
            pnlNativeResults.Controls.Add(noResultsLabel);
        }

        private void ShowError(Exception ex)
        {
            string errorHtml = $@"
<html>
<body>
    <div style='color: red; padding: 20px;'>
        <h3>Error</h3>
        <p>{System.Net.WebUtility.HtmlEncode(ex.Message)}</p>
        <pre style='background-color: #f5f5f5; padding: 10px; border: 1px solid #ddd;'>{System.Net.WebUtility.HtmlEncode(ex.StackTrace)}</pre>
    </div>
</body>
</html>";
            webBrowser.Html = errorHtml;

            pnlNativeResults.Controls.Clear();
            var errorLabel = new Label
            {
                Text = $"Error: {ex.Message}",
                ForeColor = System.Drawing.Color.Red,
                AutoSize = true,
                Location = new System.Drawing.Point(10, 10),
                MaximumSize = new System.Drawing.Size(pnlNativeResults.Width - 20, 0)
            };
            pnlNativeResults.Controls.Add(errorLabel);
        }
    }
}
