Option Explicit On
Option Strict On

Imports Mongoose.IDO
Imports Mongoose.IDO.Protocol
Imports Mongoose.IDO.DataAccess
Imports Mongoose.Core.Common
Imports System.Data.SqlClient

<IDOExtensionClass("WCWCommissionReportDetail")> _
Public Class SLServerOperations
    Inherits ExtensionClassBase
    Implements IDisposable

    Private appDB As ApplicationDB = IDORuntime.Context.CreateApplicationDB()

    Public Property AppDBConnection() As ApplicationDB
        Get
            Return appDB
        End Get
        Set(value As ApplicationDB)
            appDB = value
        End Set
    End Property

    Public Function SelectData(ByVal sqlCommand As String, ByRef processID As String) As DataTable
        Dim oDataReader As IDataReader = Nothing
        Dim results As DataTable = New DataTable()
        Dim ds As DataSet = New DataSet

        Try
            Using cmd As IDbCommand = appDB.CreateCommand()
                cmd.CommandText = sqlCommand
                Dim dr As IDataReader = appDB.ExecuteReader(cmd)
                results = ConvertDataReaderToDataTable(dr)
            End Using
        Catch ex As Exception
            MGException.Throw(MGException.ExtractMessages(ex))

        End Try

        Return results
    End Function

    Public Function ConvertDataReaderToDataTable(ByVal reader As IDataReader) As DataTable
        Dim objDataTable As New DataTable
        Dim intFieldCount As Integer = reader.FieldCount
        Dim intCounter As Integer
        For intCounter = 0 To intFieldCount - 1
            objDataTable.Columns.Add(reader.GetName(intCounter), reader.GetFieldType(intCounter))
        Next intCounter

        objDataTable.BeginLoadData()
        Dim objValues(intFieldCount - 1) As Object
        While reader.Read()
            reader.GetValues(objValues)
            objDataTable.LoadDataRow(objValues, True)
        End While
        reader.Close()
        objDataTable.EndLoadData()

        Return objDataTable

    End Function

    Private Function ExecuteSp(ByVal sqlComand As IDbCommand) As Boolean
        Dim results As Boolean
        results = True
        Try
            appDB.ExecuteNonQuery(sqlComand)
        Catch ex As Exception
            results = False
            MGException.Throw(MGException.ExtractMessages(ex))
        End Try
        Return results
    End Function

    Public Function GetSQLDMOColumnValue(ByRef DataSet As DataTable, ByVal RowPos As Integer, ByVal ColPos As Integer) As String
        Dim Value As String = vbNullString
        If DataSet.Rows(RowPos)(ColPos).ToString() <> vbNullString Then
            Value = DataSet.Rows(RowPos)(ColPos).ToString()
        End If
        Return Value
    End Function


    Public Function Substitute(ByVal Str As String, ByVal PatternStr As String, ByVal ReplaceStr As String) As String
        Dim strLeft As String
        Dim strRight As String
        Dim strTemp As String
        Dim intPos As Integer

        strTemp = Str
        intPos = InStr(strTemp, PatternStr)

        Do Until intPos = 0
            strLeft = Left(strTemp, intPos - 1)
            strRight = Right(strTemp, Len(strTemp) - intPos - Len(PatternStr) + 1)
            strTemp = strLeft & ReplaceStr & strRight
            intPos = InStr(intPos + Len(ReplaceStr), strTemp, PatternStr)
        Loop

        Substitute = strTemp
    End Function

End Class
