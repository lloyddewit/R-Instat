﻿' R- Instat
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

Imports instat.Translations


Public Class dlgReferenceLevel
    Private bFirstLoad As Boolean = True
    Public strDefaultDataFrame As String = ""
    Private bReset As Boolean = True
    Private clsSetRefLevel, clsDummyFunction As New RFunction
    Private _strSelectedColumn As String

    Private Sub dlgReferenceLevel_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If bFirstLoad Then
            InitialiseDialog()
            bFirstLoad = False
        End If
        If bReset Then
            SetDefaults()
        End If
        SetRCodeforControls(bReset)
        SetSelectedColumn()
        bReset = False
        TestOKEnabled()
        autoTranslate(Me)
    End Sub

    Private Sub InitialiseDialog()
        ucrBase.iHelpTopicID = 38

        ucrSelectorForReferenceLevels.SetParameter(New RParameter("data_name", 0))
        ucrSelectorForReferenceLevels.SetParameterIsString()

        ucrReceiverReferenceLevels.SetParameter(New RParameter("col_name", 1))
        ucrReceiverReferenceLevels.SetParameterIsString()
        ucrReceiverReferenceLevels.Selector = ucrSelectorForReferenceLevels
        ucrReceiverReferenceLevels.SetMeAsReceiver()
        ucrReceiverReferenceLevels.SetIncludedDataTypes({"factor"}, bStrict:=True)
        ucrReceiverReferenceLevels.strSelectorHeading = "Factors"
        ucrReceiverReferenceLevels.SetExcludedDataTypes({"ordered,factor"})


        Dim dctParamAndColNames As New Dictionary(Of String, String)
        dctParamAndColNames.Add("new_ref_level", ucrFactor.DefaultColumnNames.Label)

        ucrFactorReferenceLevels.SetParameter(New RParameter("new_ref_level", 2))
        ucrFactorReferenceLevels.SetAsSingleSelectorGrid(ucrReceiverReferenceLevels,
                                                  dctParamAndColNames:=dctParamAndColNames,
                                                  hiddenColNames:={ucrFactor.DefaultColumnNames.Level},
                                                  bIncludeNALevel:=False)

    End Sub

    Private Sub SetDefaults()
        clsSetRefLevel = New RFunction
        clsDummyFunction = New RFunction
        ucrSelectorForReferenceLevels.Reset()

        ucrReceiverReferenceLevels.SetMeAsReceiver()
        clsDummyFunction.AddParameter("strVal", ucrReceiverReferenceLevels.GetVariableNames(False))
        clsSetRefLevel.SetRCommand(frmMain.clsRLink.strInstatDataObject & "$set_factor_reference_level")
        ucrBase.clsRsyntax.SetBaseRFunction(clsSetRefLevel)
    End Sub

    Private Sub SetRCodeforControls(bReset As Boolean)
        ucrSelectorForReferenceLevels.SetRCode(clsSetRefLevel, bReset)
        ucrReceiverReferenceLevels.SetRCode(clsSetRefLevel, bReset)
        ucrFactorReferenceLevels.SetRCode(clsSetRefLevel, bReset)
    End Sub

    Private Sub TestOKEnabled()
        ucrBase.OKEnabled(Not ucrReceiverReferenceLevels.IsEmpty AndAlso ucrFactorReferenceLevels.IsAnyGridRowSelected)
    End Sub

    Private Sub ucrBase_ClickReset(sender As Object, e As EventArgs) Handles ucrBase.ClickReset
        SetDefaults()
        SetRCodeforControls(True)
        TestOKEnabled()
    End Sub

    Public Property SelectedColumn As String
        Get
            Return _strSelectedColumn
        End Get
        Set(value As String)
            _strSelectedColumn = value
        End Set
    End Property

    Private Sub SetSelectedColumn()
        ' Call the utility method to perform the column selection logic.
        clsColumnSelectionUtility.SetSelectedColumn(ucrSelectorForReferenceLevels.lstAvailableVariable,
                                                 ucrReceiverReferenceLevels,
                                                 clsDummyFunction,
                                                 ucrSelectorForReferenceLevels.strCurrentDataFrame,
                                                 _strSelectedColumn)
    End Sub

    Private Sub ucrControls_ControlValueChanged(ucrChangedControl As ucrCore) Handles ucrReceiverReferenceLevels.ControlValueChanged, ucrFactorReferenceLevels.ControlValueChanged
        TestOKEnabled()
        ucrReceiverReferenceLevels.SetMeAsReceiver()
        clsDummyFunction.AddParameter("strVal", ucrReceiverReferenceLevels.GetVariableNames(False))
    End Sub
End Class