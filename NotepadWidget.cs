using System;
using Sandbox.UI;
using System.Text.RegularExpressions;
using Button = Editor.Button;
using FileSystem = Sandbox.FileSystem;
using Label = Editor.Label;

[Dock("Editor", "Notepad", "notes")]
public class NotepadWidget : Widget
{
	private Dialog createNoteDialog;
	private ListView notesListView;
	private TextEdit textEdit;
	private Button saveButton;
	private Button removeNoteButton;
	private Label noteNameLabel;

	private Layout LeftLayout;
	private Layout RightLayout;

	private string workingFile;

	private Vector2 MaxDialogWindowSize = new Vector2( 355, 120 );
	
	public NotepadWidget(Widget parent) : base(parent, true)
	{
		CreateNotesDirectory();
		
		// Create Main Layout
		Layout = Layout.Row();
		Layout.Margin = 4;
		Layout.Spacing = 4;
		
		// Create Left Layout
		// (for ListView with notes and buttons to create or remove note)
		LeftLayout = Layout.AddColumn();
		LeftLayout.Margin = 4;
		LeftLayout.Spacing = 4;
		
		// Create Right Layout
		// (for TextEdit and button to save note)
		RightLayout = Layout.AddColumn();
		RightLayout.Margin = 4;
		RightLayout.Spacing = 4;
		
		SetStyles( "color: white; font-weight: 600;" );

		CreateListView();
		CreateRemoveNoteButtons();
		CreateTextEditor();
		CreateSaveButton();
		
		RefreshListView();
	}
	
	/// <summary>
	/// Creates notes directory if it doesn't exist.
	/// </summary>
	private static void CreateNotesDirectory()
	{
		if ( !FileSystem.Data.DirectoryExists( "notes" ) )
		{
			FileSystem.Data.CreateDirectory( "notes" );
			Log.Info( $"Created notes directory." );
		}
	}
	
	/// <summary>
	/// Displays a dialog where user can create new note.
	/// </summary>
	private void CreateNoteDialog()
	{
		createNoteDialog = new Dialog( this );
		createNoteDialog.WindowTitle = "Create Note";
		createNoteDialog.SetStyles( "color: white; font-weight: 600;" );
		
		var layout = createNoteDialog.Layout = Layout.Column();

		var textNoteName = layout.Add( new LineEdit( this ) );
		textNoteName.Alignment = TextFlag.Center;
		textNoteName.PlaceholderText = "My New Note";
		
		var createButton =  layout.Add( new Button( "Create", this ) );
		textNoteName.SetSizeMode( SizeMode.Default, SizeMode.Ignore );
		createButton.Clicked += () =>
		{
			var text = textNoteName.Text;
			var pattern =
				@"^[\w\-. ]+$";
			
			if (Regex.IsMatch( text, pattern ))
			{
				Save(textNoteName.Text, String.Empty);
				RefreshListView();
				OnNoteDeselected();
				createNoteDialog.Close();
			}
			else
			{
				Log.Error( $"Invalid file name!" );
			}
		};

		createNoteDialog.Window.MaximumSize = MaxDialogWindowSize;
		createNoteDialog.Show();
	}

	private void RemoveNote()
	{
		if (!IsNoteSelected()) return;
		
		FileSystem.Data.DeleteFile( FileSystem.NormalizeFilename( $"notes/{workingFile}.txt" ) );
		RefreshListView();
		OnNoteDeselected();
	}

	#region RIGHT_LAYOUT
	private void CreateTextEditor()
	{
		noteNameLabel = RightLayout.Add( new Label( this ) );
		noteNameLabel.Text = "Select Note";
		textEdit = RightLayout.Add(new TextEdit(this) );
	}

	private void CreateSaveButton()
	{
		saveButton = RightLayout.Add( new Button( "Save", this ) );
		saveButton.Enabled = false;
		saveButton.Clicked += () =>
		{
			Save( workingFile,  textEdit.PlainText );
		};
	}
	#endregion

	#region LEFT_LAYOUT
	private void CreateListView()
	{
		notesListView = LeftLayout.Add( new ListView( this ) );
		notesListView.SetSizeMode( SizeMode.Ignore, SizeMode.CanGrow );
		notesListView.MaximumWidth = 180f;
		notesListView.ItemSize = new Vector2( 150, 50 );
		notesListView.ItemDeselected += (item) =>
		{
			if ( item.ToString() == null )
			{
				OnNoteDeselected();
			}
		};
			
		notesListView.ItemClicked += ( item ) =>
		{
			workingFile = item.ToString();

			if ( workingFile == null )
			{
				OnNoteDeselected();
				return;
			}
			
			noteNameLabel.Text = workingFile;
			textEdit.PlainText = FileSystem.Data.ReadAllText( FileSystem.NormalizeFilename( $"notes/{workingFile}.txt" ) );
			saveButton.Enabled = true;
			removeNoteButton.Enabled = true;
		};
	}
	private void CreateRemoveNoteButtons()
	{
		var createNoteButton = LeftLayout.Add( new Button("Create Note", this));
		createNoteButton.ToolTip = "Create new note.";
		createNoteButton.Clicked += CreateNoteDialog;
		
		removeNoteButton = LeftLayout.Add( new Button("Remove Note", this));
		removeNoteButton.ToolTip = "Remove selected note.";
		removeNoteButton.Clicked += RemoveNote;
		removeNoteButton.Enabled = false;
	}
	#endregion

	private void OnNoteDeselected()
	{
		workingFile = String.Empty;
		textEdit.PlainText = String.Empty;
		noteNameLabel.Text = String.Empty;
		saveButton.Enabled = false;
		removeNoteButton.Enabled = false;
	}

	private void RefreshListView()
	{
		notesListView.Clear();
		foreach ( var file in FileSystem.Data.FindFile( "notes", "*.txt" ) )
		{
			notesListView.AddItem( file.Replace( ".txt", "" ) );
		}
	}

	private void Save(string noteTitle, string content)
	{
		FileSystem.Data.WriteAllText( FileSystem.NormalizeFilename( $"notes/{noteTitle}.txt" ), content );
		Log.Info( $"Saved to {FileSystem.Data.GetFullPath( FileSystem.NormalizeFilename( $"notes/{noteTitle}.txt" ) )}" );
	}

	private bool IsNoteSelected()
	{
		return workingFile == String.Empty;
	}
	
	/// <summary>
	/// This is a shortcut method,
	/// it detects if CTRL+S was pressed (a save shortcut).
	/// </summary>
	/// <param name="e"></param>
	protected override void OnShortcutPressed( KeyEvent e )
	{
		base.OnShortcutPressed(e);
		if ( e.HasCtrl && e.Key == KeyCode.S )
		{
			if ( workingFile != null )
			{
				Save( workingFile, textEdit.PlainText );
			}
		}
	}
}
