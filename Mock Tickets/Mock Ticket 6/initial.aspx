<div id="mdialog-addanalysis" title="" style="padding: 20px;">
    <div id="mdialog-addanalysis-content" style="height: 400px; overflow-y: auto;">
        <table>
            <tr>
                <td>
                    <b>Section:</b>
                </td>
                <td>
                    <PLC:FlexBox ID="fbSection" runat="server" TableName="TV_EXAMCODE">
                    </PLC:FlexBox>
                </td>
            </tr>
        </table>
        <br />
        <div id="satc"><span style="color: Blue;">Select a section.</span></div>
    </div>
</div>

<div class="hide">
    <div class="dashboard-panels">
        <div class="dashboard-panels-item">
            <div>
                <b>Dashboard Panels</b>
                <ul id="dashboardPanelList" class="dashboard-panels-item"></ul>
            </div>
        </div>
        Drag & drop to sort the dashboard panels.
    </div>
</div>

<div id="mdialog-sortanalysis" title="" style="padding: 20px;">
    <div id="mdialog-sortanalysis-content" style="height: 400px; overflow-y: auto;">
        <table>
            <tr>
                <td>
                    <b>Section:</b>
                </td>
                <td>
                    <PLC:FlexBox ID="fbSection" runat="server" TableName="TV_EXAMCODE">
                    </PLC:FlexBox>
                </td>
            </tr>
        </table>
        <br />
        <div id="satc"><span style="color: Blue;">Select a section.</span></div>
    </div>
</div>

--
<div id="pnlAPS">
    <div style="overflow-y: scroll; height: 180px;">
        <select id="pnlAnalysisSort" style="width: 350px" size="3">
            <option value="001,Chemical">Chemical Analysis</option>
            <option value="002,DNA">DNA Testing</option>
            <option value="003,Toxicology">Toxicology</option>
        </select>
    </div>
    <center>
        <div>
            <input type="button" id="btnMoveAnalysisUp" value="Move Panel Up" onclick="btnMoveAnalysisUp_Click()">
            <input type="button" id="btnMoveAnalysisDown" value="Move Panel Down" onclick="btnMoveAnalysisDown_Click()">
        </div>
    </center>
</div>