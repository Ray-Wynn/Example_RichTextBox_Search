# Example_RichTextBox_Search
A simple, no frills, example of how to obtain the TextRange of text (the first occurrence)
in a Rich Text Format (rtf) file.

Something to get started with and build on...

RichEditBox uses a FlowDocument to represent its contents including images, formatting and text. 
There is no FlowDocument method to find the TextPointer position of text.
	TextRange.Text.IndexOf(string, start) looks promising, 
		but only contains plain text ignoring all other FlowDocument content.

	TextPointer and TextRange refer to a position in the FlowDocument that may or may not include text!

To find the TextPointer position of text requires indexing through the FlowDocument.
This can be very slow, especially if the document contains formatting and image content.

To speed up searching, first check each FlowDocument block for (Contains) the searched for string.
	
	foreach (Block block in rtb.Document.Blocks)
	{		
		searchRange.Select(block.ContentStart, block.ContentEnd);
                
		if (searchRange.Text.Contains(searchForComparison, stringComparison))
		{			
			// Then using TextPointer indexing search for the string in the block.
		}
	}