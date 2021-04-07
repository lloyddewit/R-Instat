' R- Instat
' Copyright (C) 2015-2017
'
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License 
' along with this program.  If not, see <http://www.gnu.org/licenses/>.
Imports System.Reflection
Imports System.Text.RegularExpressions
'Imports instat.My.MyProject

Public Class Translations

    '**********************************************************************************************
    'TODO This section contains functions for the new translation system started in March 2021.
    ' These are prototype functions and are still under development.
    '**********************************************************************************************

    '''--------------------------------------------------------------------------------------------
    ''' <summary>
    '''     TODO this function is still under development - please do not peer review or test yet.
    '''     Attempts to translate all the text in <paramref name="ctrParent"/> to the currently
    '''     configured language.
    ''' </summary>
    '''
    ''' <param name="ctrParent">    The WinForm control to translate. </param>
    ''' <param name="CultureInfo">  (Optional) Not used. Included only for historical reasons. </param>
    '''--------------------------------------------------------------------------------------------
    Public Shared Sub autoTranslate(ctrParent As Control, Optional CultureInfo As Globalization.CultureInfo = Nothing)
        If IsNothing(TryCast(ctrParent, Form)) Then
            Exit Sub
        End If

        Dim strErrorMsg As String = TranslateWinForm.clsTranslateWinForm.translateForm(ctrParent, GetDbPath(), GetLanguageCode())
        If Not String.IsNullOrEmpty(strErrorMsg) Then
            MsgBox(strErrorMsg, MsgBoxStyle.Exclamation)
        End If
    End Sub

    '''--------------------------------------------------------------------------------------------
    ''' <summary>
    '''     Attempts to translate all the text in the menu items in <paramref name="tsCollection"/> 
    '''     to  the currently configured language.
    ''' </summary>
    '''
    ''' <param name="tsCollection">     The WinForm menu items to translate. </param>
    ''' <param name="ctrParent">        The WinForm control that is the parent of the menu. </param>
    '''--------------------------------------------------------------------------------------------
    Public Shared Sub translateMenu(tsCollection As ToolStripItemCollection, ctrParent As Control)
        ' TODO Lloyd 01/04/21 The function 'ExportControls' below generates, for every form in the 
        ' application, a csv file containing the form name, the names of the form's controls, and 
        ' the text of the form's controls.
        ' The CSV file can be imported into the translations database.
        ' The function below only needs to be executed once per release.
        WriteCsvFile()

        'TODO
        autoTranslate(ctrParent)

        Dim strErrorMsg As String = TranslateWinForm.clsTranslateWinForm.translateMenuItems(tsCollection, ctrParent, GetDbPath(), GetLanguageCode())
        If Not String.IsNullOrEmpty(strErrorMsg) Then
            MsgBox(strErrorMsg, MsgBoxStyle.Exclamation)
        End If
    End Sub

    '''--------------------------------------------------------------------------------------------
    ''' <summary>   Returns the language code for the currently configured language (e.g. 'en' for 
    '''             English, 'fr' for French etc.). </summary>
    '''
    ''' <returns>   The language code for the currently configured language (e.g. 'en' for
    '''             English, 'fr' for French etc.). </returns>
    '''--------------------------------------------------------------------------------------------
    Private Shared Function GetLanguageCode() As String
        Dim arrLanguageCodes As String() = frmMain.clsInstatOptions.strLanguageCultureCode.Split(New Char() {"-"c})
        Dim strLanguageCode As String = arrLanguageCodes(0)
        Return strLanguageCode
    End Function

    '''--------------------------------------------------------------------------------------------
    ''' <summary>   Returns the full path of the SQLite translations database file. </summary>
    '''
    ''' <returns>   The full path of the SQLite translations database file. </returns>
    '''--------------------------------------------------------------------------------------------
    Private Shared Function GetDbPath() As String
        Dim strTranslationsPath As String = String.Concat(System.Windows.Forms.Application.StartupPath, "\translations")
        Dim strDbFile As String = "rInstatTranslations.db"
        Dim strDbPath As String = System.IO.Path.Combine(strTranslationsPath, strDbFile)
        Return strDbPath
    End Function

    ''' <summary>   TODO. </summary>
    Private Shared Sub WriteCsvFile()

        'Get list of all form classes in the application 
        '    (specifically, a list of 'Type' objects, each 'Type' object contains details about 
        '    a class)
        Dim clsAssembly As Assembly = Assembly.GetExecutingAssembly()
        Dim lstFormClasses As List(Of Type) = clsAssembly.GetTypes().Where(Function(t) t.BaseType = GetType(Form)).ToList()

        'Populate the csv string for each form in the project
        'Note: We know the name of each form class (see list above). We also know that 
        '      the 'My.Forms' object contains an object for each form class.
        '      Conveniently, the name of each object in 'My.Forms' is the same as the name of 
        '      the object's class. 
        '      Therefore we can use the class name as the object name in 'CallByName'.
        Dim strControlsAsCsv As String = ""
        For Each typFormClass As Type In lstFormClasses
            Dim frmTemp As Form = CallByName(My.Forms, typFormClass.Name, CallType.Get)
            strControlsAsCsv &= GetControlsAsCsv(frmTemp, frmTemp)
        Next

        'TODO The right mouse button menus for the 6 output windows are not accessible via 
        '     the control lists.
        '     Therefore, add these manually to the CSV file
        strControlsAsCsv &= GetMenuItemsAsCsv(frmMain.ucrOutput, frmMain.ucrOutput.mnuContextRTB.Items)

        'Write the csv file
        Dim strDesktopPath As String = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        Dim strFileName As String = "form_controls.csv"
        Dim strPath As String = System.IO.Path.Combine(strDesktopPath, strFileName)
        Using sw As New System.IO.StreamWriter(strPath)
            Console.WriteLine(strControlsAsCsv)
            sw.WriteLine(strControlsAsCsv)
            sw.Flush()
            sw.Close()
        End Using

        'This sub should only be used by developers to create the translation export files.
        'Therefore, exit the application with a message to ensure that this sub is not run 
        'accidentally in the release version. 
        MsgBox("The form controls' translation text was written to: " & strPath &
               ". The application will now exit.", MsgBoxStyle.Exclamation)
        System.Windows.Forms.Application.Exit()
    End Sub

    '''--------------------------------------------------------------------------------------------
    ''' <summary>   TODO. </summary>
    '''
    ''' <param name="ctrParent">    The WinForm control that is the parent of the menu. </param>
    ''' <param name="ctrChild">     The counter child. </param>
    '''
    ''' <returns>   The controls as CSV. </returns>
    '''--------------------------------------------------------------------------------------------
    Private Shared Function GetControlsAsCsv(ctrParent As Control, ctrChild As Control) As String
        If IsNothing(ctrParent) OrElse IsNothing(ctrChild) OrElse
                TypeOf ctrChild Is ucrCheck OrElse TypeOf ctrChild Is ucrInput Then
            Return ""
        End If

        Dim strControlsAsCsv As String = ""
        Dim arrRNewLines() As String = {vbCr, vbLf, vbCrLf}
        If Not (String.IsNullOrEmpty(ctrParent.Name) OrElse
                String.IsNullOrEmpty(ctrChild.Name) OrElse
                String.IsNullOrEmpty(ctrChild.Text) OrElse
                ctrChild.Text.Contains(vbCr) OrElse ctrChild.Text.Contains(vbLf) OrElse 'ignore multiline text
                Not Regex.IsMatch(ctrChild.Text, "[a-zA-Z]")) Then 'ignore text that doesn't contain letters (e.g. number strings)
            strControlsAsCsv = ctrParent.Name & "," & ctrChild.Name & "," & ctrChild.Text & vbCrLf
        End If

        If TypeOf ctrChild Is ContextMenuStrip Then
            Dim mnuTmp As ContextMenuStrip = DirectCast(ctrChild, ContextMenuStrip)
            strControlsAsCsv &= GetMenuItemsAsCsv(ctrParent, mnuTmp.Items)
        ElseIf TypeOf ctrChild Is MenuStrip Then
            Dim mnuTmp As MenuStrip = DirectCast(ctrChild, MenuStrip)
            strControlsAsCsv &= GetMenuItemsAsCsv(ctrParent, mnuTmp.Items)
        ElseIf TypeOf ctrChild Is ToolStrip Then
            Dim mnuTmp As ToolStrip = DirectCast(ctrChild, ToolStrip)
            strControlsAsCsv &= GetMenuItemsAsCsv(ctrParent, mnuTmp.Items)
        Else
            For Each ctrGrandchild As Control In ctrChild.Controls
                strControlsAsCsv &= GetControlsAsCsv(ctrParent, ctrGrandchild)
            Next
        End If
        Return strControlsAsCsv
    End Function

    '''--------------------------------------------------------------------------------------------
    ''' <summary>   
    '''     Recursively traverses the <paramref name="tsCollection"/> menu hierarchy and returns a 
    '''     string containing the parent, name and associated text of each (sub)menu option in 
    '''     <paramref name="tsCollection"/>. The string is formatted as a comma-separated list 
    '''     suitable for importing into a database.
    ''' </summary>
    '''
    ''' <param name="ctrParent">        The WinForm control that is the parent of the menu. </param>
    ''' <param name="tsCollection">     The WinForm menu items to add to the return string. </param>
    '''
    ''' <returns>   
    '''     A string containing the parent and name of each (sub)menu option in
    '''     <paramref name="tsCollection"/>. The string is formatted as a comma-separated list
    '''     suitable for importing into a database. </returns>
    '''--------------------------------------------------------------------------------------------
    Private Shared Function GetMenuItemsAsCsv(ctrParent As Control, tsCollection As ToolStripItemCollection) As String
        Dim strMenuItemsAsCsv As String = ""

        For Each tsItem As ToolStripItem In tsCollection
            If tsItem.Text <> "" Then
                strMenuItemsAsCsv &= ctrParent.Name & "," & tsItem.Name & "," & tsItem.Text & vbCrLf
            End If
            If TypeOf tsItem Is ToolStripMenuItem Then
                Dim mnuItem As ToolStripMenuItem = DirectCast(tsItem, ToolStripMenuItem)
                If mnuItem.HasDropDownItems Then
                    strMenuItemsAsCsv &= GetMenuItemsAsCsv(ctrParent, mnuItem.DropDownItems)
                End If
            ElseIf TypeOf tsItem Is ToolStripSplitButton Then
                Dim mnuItem As ToolStripSplitButton = DirectCast(tsItem, ToolStripSplitButton)
                If mnuItem.HasDropDownItems Then
                    strMenuItemsAsCsv &= GetMenuItemsAsCsv(ctrParent, mnuItem.DropDownItems)
                End If
            ElseIf TypeOf tsItem Is ToolStripDropDownButton Then
                Dim mnuItem As ToolStripDropDownButton = DirectCast(tsItem, ToolStripDropDownButton)
                If mnuItem.HasDropDownItems Then
                    strMenuItemsAsCsv &= GetMenuItemsAsCsv(ctrParent, mnuItem.DropDownItems)
                End If
            End If
        Next

        Return strMenuItemsAsCsv
    End Function


    '**********************************************************************************************
    'TODO This section contains functions from the old translation system.
    ' These functions will be replaced as part of the new translation system started in March 2021.
    '**********************************************************************************************

    Public Shared Function translate(tag As String) As String
        ' Note: if the tag is not found in Resources then Nothing will be returned
        Return My.Resources.ResourceManager.GetObject(tag)
    End Function

    Public Shared Sub translateEach(controls As Control.ControlCollection, ctrParent As Control, Optional res As ComponentModel.ComponentResourceManager = Nothing, Optional CultureInfo As Globalization.CultureInfo = Nothing)
        Dim mnuTmp As MenuStrip
        Dim pntLocation As Point

        If res Is Nothing Then
            res = New ComponentModel.ComponentResourceManager(ctrParent.GetType)
        End If
        If CultureInfo Is Nothing Then
            CultureInfo = Threading.Thread.CurrentThread.CurrentUICulture
        End If
        For Each aControl As Control In controls
            'Checkbox text is set in the dialog so shouldn't be translated
            'Input controls use Text property for display value so shouldn't be translated
            If TypeOf aControl IsNot ucrCheck AndAlso TypeOf aControl IsNot ucrInput Then
                If TypeOf aControl Is MenuStrip Then
                    mnuTmp = DirectCast(aControl, MenuStrip)
                    translateMenu(mnuTmp.Items, ctrParent)
                ElseIf TypeOf aControl Is UserControl OrElse TypeOf aControl Is Panel OrElse TypeOf aControl Is GroupBox OrElse TypeOf aControl Is TabControl OrElse TypeOf aControl Is SplitContainer OrElse TypeOf aControl Is TreeView Then
                    translateEach(aControl.Controls, aControl, res, CultureInfo)
                End If
                If aControl.Name <> "" Then
                    pntLocation = aControl.Location
                    res.ApplyResources(aControl, aControl.Name, CultureInfo)
                    aControl.Location = pntLocation
                End If
            End If
        Next

        '' Given a ControlCollection, attempt to translate the Text property of each control
        'Dim formControl As Control
        'Dim originalTag As String
        'Dim translatedString As String

        'For Each formControl In controls
        '    If (TypeOf formControl Is Panel) Then
        '        ' Recursively translate all controls inside the panel
        '        translateEach(formControl.Controls)

        '    ElseIf (TypeOf formControl Is MenuStrip) Then
        '        ' Translate all MenuItems inside the MenuStrip
        '        translateMenu(formControl)


        '    ElseIf (TypeOf formControl Is UserControl) Then
        '        ' Translate all controls in a usercontrol
        '        translateEach(formControl.Controls)

        '    ElseIf (TypeOf formControl Is TextBox OrElse TypeOf formControl Is Button OrElse TypeOf formControl Is Label OrElse TypeOf formControl Is CheckBox) Then
        '        originalTag = formControl.Tag
        '        If (originalTag IsNot Nothing) Then
        '            translatedString = My.Resources.ResourceManager.GetObject(originalTag)
        '            If (translatedString IsNot Nothing) Then
        '                formControl.Text = translatedString
        '            End If
        '        End If
        '    End If
        'Next formControl
    End Sub


    '**********************************************************************************************
    'TODO This section contains old functions that have been replaced by the new translation system 
    ' started in March 2021.
    ' These functions will be deleted when the new translation system has been implemented.
    '**********************************************************************************************

    Public Shared Sub DELETEMEautoTranslate(ctrParent As Control, Optional CultureInfo As Globalization.CultureInfo = Nothing)
        'Dim translatedString As String

        '' Attempt to translate the form's title if it has a tag
        'If frm.Tag IsNot Nothing Then
        '    translatedString = My.Resources.ResourceManager.GetObject(frm.Tag)
        '    If (translatedString IsNot Nothing) Then
        '        frm.Text = translatedString
        '    End If
        'End If
        ' Translate all controls in object
        translateEach(ctrParent.Controls, ctrParent, New ComponentModel.ComponentResourceManager(ctrParent.GetType()), CultureInfo)
    End Sub

    'translateMenu And translateSubMenu should Not be neccessary if we can improve translateEach to accept any iterable
    Public Shared Sub DELETEMEtranslateMenu(tsCollection As ToolStripItemCollection, ctrParent As Control)
        Dim tsItem As ToolStripItem
        Dim mnuItem As ToolStripMenuItem
        Dim res As ComponentModel.ComponentResourceManager = New ComponentModel.ComponentResourceManager(ctrParent.GetType)

        For Each tsItem In tsCollection
            ' process this item, then recursively process any sub items
            res.ApplyResources(tsItem, tsItem.Name, Threading.Thread.CurrentThread.CurrentUICulture)
            mnuItem = TryCast(tsItem, ToolStripMenuItem)
            If mnuItem IsNot Nothing Then
                mnuItem = DirectCast(tsItem, ToolStripMenuItem)
                If mnuItem.HasDropDownItems Then
                    translateMenu(mnuItem.DropDownItems, ctrParent)
                End If
            End If
        Next
    End Sub

    '' translateMenu and translateSubMenu should not be neccessary if we can improve translateEach to accept any iterable
    'Public Shared Sub translateSubMenu(subMenuControl As ToolStripItemCollection)
    '    Dim item
    '    Dim originalTag As String
    '    Dim translatedString As String

    '    For Each item In subMenuControl
    '        ' process this item, then recursively process any sub items
    '        If Not (TypeOf item Is ToolStripSeparator) Then

    '            If (item.hasdropdownitems()) Then
    '                translateSubMenu(item.DropDownItems)
    '            End If
    '            originalTag = item.Tag
    '            If (originalTag IsNot Nothing) Then
    '                translatedString = My.Resources.ResourceManager.GetObject(originalTag)
    '                If (translatedString IsNot Nothing) Then
    '                    item.Text = translatedString
    '                End If
    '            End If
    '        End If
    '    Next item
    'End Sub
End Class
