using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.EditorInput;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using System.IO;
using Newtonsoft.Json;
using Autodesk.AutoCAD.ApplicationServices;

[assembly: CommandClass(typeof(PackageNetPlugin.Class1))]

namespace PackageNetPlugin
{
    [Serializable]
    class jsonTables
    { 
        [JsonProperty("tb")]
        public jsonTable[] tb;
    }

    [Serializable]
    class jsonTable
    {
        [JsonProperty("tbName")]
        public string tbName;

        [JsonProperty("tbRowCount")]
        public int tbRowCount;

        [JsonProperty("tbColumnCount")]
        public int tbColumnCount;

        [JsonProperty("tbRows")]
        public jsonRow[] tbRows;
    }

    [Serializable]
    class jsonRow
    {
        [JsonProperty("tbCells")]
        public jsonCell[] tbCells;
    }

    [Serializable]
    class jsonCell
    {
        [JsonProperty("tbCellStr")]
        public string tbCellStr;
    }



    public class Class1
    {
        private int rowCount = 0;
        //Current drawing
        private static Document doc = 
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
        private static Database db = doc.Database; //subclass of Document, 
        private static Editor ed = doc.Editor; //Editor object to ask user where table goes, subclass of Document
         

        [CommandMethod("MyPluginCommand")]
        public void MyPluginCommand()
        {
            TypedValue[] acTypValAr = new TypedValue[1];
             acTypValAr.SetValue(new TypedValue((int)DxfCode.Start, "ACAD_TABLE"), 0);
 
            // Assign the filter criteria to a SelectionFilter object
            SelectionFilter acSelFtr = new SelectionFilter(acTypValAr);

            // Request for objects to be selected in the drawing area
            PromptSelectionResult acSSPrompt;

            using (Transaction acTrans = db.TransactionManager.StartTransaction())
            {
                //acSSPrompt = ed.GetSelection(acSelFtr);
                acSSPrompt = ed.SelectAll(acSelFtr);



                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;

                    ed.WriteMessage("\nNumber of Tables selected: " +
                                                acSSet.Count.ToString());


                    List<jsonTable> jsonTableArray = new List<jsonTable>();

                    foreach (SelectedObject acSSObj in acSSet)
                    {
                        Entity acEnt = acTrans.GetObject(acSSObj.ObjectId,
                                                  OpenMode.ForRead) as Entity;

                        Table oTB = acEnt as Table;

                        ed.WriteMessage("\nGetting Data from Table:{0}", oTB.Name);

                        int Rows = oTB.Rows.Count;
                        int Cols = oTB.Columns.Count;

                        var jsonTable = new jsonTable()
                        {
                            tbName = oTB.Name,
                            tbRowCount = Rows,
                            tbColumnCount = Cols
                        };

                        List<jsonRow> jsonRowArray = new List<jsonRow>();


                        for (int row = 0; row < Rows; row++)
                        {
                            jsonRow eachRow = new jsonRow();

                            List<jsonCell> jsonCellArray = new List<jsonCell>();

                            for (int col = 0; col < Cols; col++)
                            {
                                jsonCell eachCell = new jsonCell();
                                eachCell.tbCellStr =
                                         oTB.Cells[row, col].GetTextString(FormatOption.FormatOptionNone);

                                jsonCellArray.Add(eachCell);


                            }
                            eachRow.tbCells = jsonCellArray.ToArray<jsonCell>();
                            jsonRowArray.Add(eachRow); 
                        }

                        jsonTable.tbRows = jsonRowArray.ToArray<jsonRow>();
                        jsonTableArray.Add(jsonTable);

                    }

                    var jsonTableInstance = new jsonTables()
                    {
                        tb = jsonTableArray.ToArray<jsonTable>()
                    };

                    string json_data = JsonConvert.SerializeObject(jsonTableInstance);

                    var jsonOut = Path.Combine("myTableData.json");
                    FileStream fs = new FileStream(jsonOut, FileMode.Create);
                    StreamWriter sw = new StreamWriter(fs);
                    try
                    {
                        sw.Write(json_data);
                        sw.Flush();
                        
                        ed.WriteMessage("\nWrite Json File Succeeded! " );

                    }
                    catch (System.Exception ex)
                    {
                        ed.WriteMessage("\nWrite Json File Error: " + ex.Message.ToString());
                    }
                    finally
                    {
                        sw.Close();
                        fs.Close();
                    } 

                }
                else
                {
                    ed.WriteMessage("NO Table Object in this Drawing!");
                }
              } 

        }
    }
}
