var SaveImagePlugin = {
    saveImageToLocal: function(link)
    {
        var url = Pointer_stringify(link);
        document.onmouseup = function()
        {
            function getFormattedTimestamp() {
			  return new Date(Date.now() - 60000 * new Date().getTimezoneOffset()).toISOString().replace(/\..*/g, '').replace(/T/g, ' ').replace(/:/g, '-');
			}
			 
			var button = document.createElement("a");
			button.setAttribute("href", url);
			button.setAttribute("download", "Screenshot " + getFormattedTimestamp() + ".png");
			button.style.display = "none";
			document.body.appendChild(button);
			button.click();
			document.body.removeChild(button);

            document.onmouseup = null;
        }
    }
};
 
mergeInto(LibraryManager.library, SaveImagePlugin);