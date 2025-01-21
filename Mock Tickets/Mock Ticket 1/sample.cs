private void SampleItem()
{
    ViewState["SAMPLE_ITEM_NEW_CONTAINER"] = true;      //Set the ViewState to true to initialize a new container
    ParentECN = PLCSession.PLCGlobalECN;                //Set the Parent ECN to the Global ECN(selected item)
    PLCButtonPanel1.ClickAddButton();                   //Click the Add Button from the Button Panel1 
    ParentECN = "";
}