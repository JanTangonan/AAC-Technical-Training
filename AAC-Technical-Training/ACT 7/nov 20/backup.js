function SelectRow() {
    $('#gridView').on('click', 'tr', function () {
        $(this).addClass('selected').siblings().removeClass('selected');

        // Get selected row data
        selectedRecord = {
            CASE_KEY: $(this).data('caseKey'),
            DEPARTMENT_CASE_NUMBER: $(this).data('departmentCaseNumber'),
            DEPARTMENT_CODE: $(this).data('departmentCode'),
            OFFENSE_CODE: $(this).data('offenseCode'),
            LAB_CASE: $(this).data('labCase'),
            OFFENSE_DATE: $(this).data('offenseDate')
        };

        console.log(selectedRecord);
    });
}

function renderGrid() {
    const table = $('<table id="gridView"></table>');
    const thead = `<thead>
                    <tr>
                        <th>Department Case #</th>
                        <th>Department Name</th>
                        <th>Charge</th>
                        <th>Lab Case #</th>
                        <th>Incedent Report Date</th>
                    </tr>
                  </thead>`;
    const tbody = $('<tbody></tbody>');

    settings.data.forEach(item => {
        const row = `<tr data-case-key="${item.CASE_KEY}" data-department-case-number="${item.DEPARTMENT_CASE_NUMBER}" data-department-code="${item.DEPARTMENT_CODE}" 
                     data-offense-code="${item.OFFENSE_CODE}" data-lab-case="${item.LAB_CASE}" data-offense-date="${item.OFFENSE_DATE}">
                        <td>${item.DEPARTMENT_CASE_NUMBER}</td>
                        <td>${item.DEPARTMENT_NAME}</td>
                        <td>${item.CHARGE}</td>
                        <td>${item.LAB_CASE}</td>
                        <td>${item.OFFENSE_DATE}</td>
                     </tr>`;
        tbody.append(row);
    });

    table.append(thead).append(tbody);
    $container.empty().append(table);
}