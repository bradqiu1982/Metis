/**
 * @license Copyright (c) 2003-2017, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.md or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function( config ) {
	// Define changes to default configuration here.
	// For complete reference see:
	// http://docs.ckeditor.com/#!/api/CKEDITOR.config

	// The toolbar groups arrangement, optimized for two toolbar rows.
	config.toolbarGroups = [
		{ name: 'clipboard',   groups: [ 'clipboard', 'undo' ] },
		{ name: 'editing',     groups: [ 'find', 'selection', 'spellchecker' ] },
		{ name: 'links' },
        { name: 'insert'},
		//{ name: 'forms' },
		{ name: 'tools' },
		{ name: 'document',	   groups: [ 'mode' ] },
		//{ name: 'others' },
		//'/',
		{ name: 'basicstyles', groups: [ 'basicstyles', 'cleanup' ] },
		{ name: 'paragraph',   groups: [ 'list', 'indent', 'blocks', 'align' ] },
        { name: 'styles' },
		{ name: 'colors' },
		{ name: 'about' }
    ];
	// Remove some buttons provided by the standard plugins, which are
	// not needed in the Standard(s) toolbar.
	config.removeButtons = 'Underline,Subscript,Superscript,Image';

	// Set the most common block elements.
	config.format_tags = 'p;h1;h2;h3;pre';

	// Simplify the dialog windows.
    config.removeDialogTabs = 'image:advanced;link:advanced';

	config.pasteFromWordRemoveStyles = false;
    config.pasteFromWordRemoveFontStyles = false;
    config.ForcePasteAsPlainText = false;
    config.image_previewText = '';

    config.allowedContent = {
        $1: {
            // Use the ability to specify elements as an object.
            elements: CKEDITOR.dtd,
            attributes: true,
            styles: true,
            classes: true
        }
    };
    config.disallowedContent = 'script; *[on*]';
    config.pasteFilter = null;
    config.uploadUrl = '/userfiles/Upload.ashx?command=QuickUpload&type=Files&responseType=json';
    //config.filebrowserUploadUrl = '/userfiles/Upload.ashx?command=QuickUpload';
    config.extraPlugins = 'html5video';
    config.extraPlugins = 'uploadfile';
};
 