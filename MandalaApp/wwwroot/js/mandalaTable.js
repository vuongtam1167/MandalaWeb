var undoHistory = [];
var redoHistory = [];
var deletedIds = [];
var $templateRow = null; // Dòng mẫu dùng để clone khi thêm row
var levelOrder = []; // Mảng chứa thứ tự target dựa trên targetOptions

// Hàm debounce
function debounce(fn, delay) {
    var timer;
    return function () {
        var context = this, args = arguments;
        clearTimeout(timer);
        timer = setTimeout(function () { fn.apply(context, args); }, delay);
    };
}

// Lấy dữ liệu của 1 dòng
function getRowData($row) {
    var idVal = $row.find("input.hiddenID").val();
    // Lấy giá trị từ hiddenMandalaLv hoặc từ select.targetSelect nếu không có
    var mandalaLvStr = $row.find("input.hiddenMandalaLv").val() || $row.find("select.targetSelect").val() || "";
    // Chuyển sang số, nếu không chuyển được thì mặc định là 0
    var mandalaLv = parseInt(mandalaLvStr, 10);
    if (isNaN(mandalaLv)) {
        mandalaLv = 0;
    }

    return {
        ID: idVal === "" ? 0 : idVal,
        Target: $row.find("select.targetSelect").val() || "",
        MandalaLv: mandalaLv,
        Deadline: $row.find("input[type='date']").val() || "",
        Status: $row.find("input[type='checkbox']").prop("checked") || false,
        Action: $row.find("input[name$='.Action']").val() || "",
        Result: $row.find("input[name$='.Result']").val() || "",
        Person: $row.find("input[name$='.Person']").val() || ""
    };
}

// Lấy trạng thái của toàn bộ bảng
function getFormState() {
    var state = [];
    $("#excelTable tbody tr").each(function () { state.push(getRowData($(this))); });
    return state;
}

// Gán dữ liệu cho 1 dòng
function setRowData($row, data) {
    $row.find("input.hiddenID").val(data.ID);
    $row.find("select.targetSelect").val(data.Target);
    // Nếu có trường MandalaLv trong dữ liệu, dùng nó; nếu không, gán giá trị của Target
    if (data.MandalaLv && data.MandalaLv !== "") {
        $row.find("input.hiddenMandalaLv").val(data.MandalaLv);
    } else {
        $row.find("input.hiddenMandalaLv").val(data.Target);
    }
    $row.find("input[type='date']").val(data.Deadline);
    $row.find("input[type='checkbox']").prop("checked", data.Status);
    $row.find("input[name$='.Action']").val(data.Action);
    $row.find("input[name$='.Result']").val(data.Result);
    $row.find("input[name$='.Person']").val(data.Person);
}

// Lấy trạng thái hoàn chỉnh (bảng và deletedIds)
function getCompleteState() {
    return { tableState: getFormState(), deletedIds: deletedIds.slice() };
}

// Gán lại trạng thái cho bảng
function setFormState(stateObj) {
    var tableState = stateObj.tableState;
    deletedIds = stateObj.deletedIds.slice();
    var $tbody = $("#excelTable tbody");
    $tbody.empty();
    for (var i = 0; i < tableState.length; i++) {
        var $newRow;
        if ($templateRow) {
            $newRow = $templateRow.clone();
        }
        else {
            $newRow = $("<tr>" +
                "<td><span class='rowIndex'></span><input type='hidden' name='[0].ID' class='hiddenID' /></td>" +
                "<td>" +
                "<select name='[0].Target' class='targetSelect'>" +
                "<option value=''>Chọn chỉ tiêu</option>" +
                "</select>" +
                "<input type='hidden' name='[0].MandalaLv' class='hiddenMandalaLv' />" +
                "</td>" +
                "<td><input type='date' name='[0].Deadline' class='form-control' /></td>" +
                "<td><input type='checkbox' name='[0].Status' /></td>" +
                "<td><input type='text' name='[0].Action' class='form-control' /></td>" +
                "<td><input type='text' name='[0].Result' class='form-control' /></td>" +
                "<td><input type='text' name='[0].Person' class='form-control' /></td>" +
                "</tr>");
            $templateRow = $newRow.clone();
        }
        $newRow.find("input, select, textarea").each(function () {
            var $input = $(this), name = $input.attr("name");
            if (name) { name = name.replace(/\[\d+\]/, "[" + i + "]"); $input.attr("name", name); }
        });
        setRowData($newRow, tableState[i]);
        $tbody.append($newRow);
    }
    autoSortTable();
}

// Hàm tự động sắp xếp bảng theo thứ tự target (theo levelOrder) và Deadline
function autoSortTable() {
    var $tbody = $("#excelTable tbody");
    var rows = $tbody.find("tr").get();
    rows.sort(function (a, b) {
        // Lấy giá trị MandalaLv đã gán
        var lvA = $(a).find("input.hiddenMandalaLv").val();
        var lvB = $(b).find("input.hiddenMandalaLv").val();
        var indexA = levelOrder.indexOf(lvA);
        var indexB = levelOrder.indexOf(lvB);
        if (indexA === -1) indexA = Number.MAX_SAFE_INTEGER;
        if (indexB === -1) indexB = Number.MAX_SAFE_INTEGER;
        if (indexA !== indexB) { return indexA - indexB; }
        else {
            var deadlineA = $(a).find("input[type='date']").val();
            var deadlineB = $(b).find("input[type='date']").val();
            var dateA = deadlineA ? new Date(deadlineA) : new Date(8640000000000000);
            var dateB = deadlineB ? new Date(deadlineB) : new Date(8640000000000000);
            return dateA - dateB;
        }
    });
    $.each(rows, function (index, row) {
        $(row).find("td:first-child span.rowIndex").text(index + 1);
        $tbody.append(row);
    });
}

// Lưu trạng thái vào undoHistory
function pushState() {
    var currentState = JSON.stringify(getCompleteState());
    if (undoHistory.length === 0 || undoHistory[undoHistory.length - 1] !== currentState) {
        undoHistory.push(currentState);
        redoHistory = [];
    }
}

function undo() {
    if (undoHistory.length > 1) {
        redoHistory.push(undoHistory.pop());
        var prevState = JSON.parse(undoHistory[undoHistory.length - 1]);
        setFormState(prevState);
    } else { alert("Không còn bước Undo!"); }
}

function redo() {
    if (redoHistory.length > 0) {
        var nextState = JSON.parse(redoHistory.pop());
        undoHistory.push(JSON.stringify(nextState));
        setFormState(nextState);
    } else { alert("Không còn bước Redo!"); }
}

// Các hàm liên quan đến MandalaLv và nhóm (giữ nguyên như cũ)
function isInGroup(mandalaLv, group) {
    switch (group) {
        case "M0":
            return mandalaLv >= 1 && mandalaLv <= 9;
        case "M1":
            return mandalaLv === 2 || (mandalaLv >= 10 && mandalaLv <= 17);
        case "M2":
            return mandalaLv === 3 || (mandalaLv >= 18 && mandalaLv <= 25);
        case "M3":
            return mandalaLv === 4 || (mandalaLv >= 26 && mandalaLv <= 33);
        case "M4":
            return mandalaLv === 5 || (mandalaLv >= 34 && mandalaLv <= 41);
        case "M5":
            return mandalaLv === 6 || (mandalaLv >= 42 && mandalaLv <= 49);
        case "M6":
            return mandalaLv === 7 || (mandalaLv >= 50 && mandalaLv <= 57);
        case "M7":
            return mandalaLv === 8 || (mandalaLv >= 58 && mandalaLv <= 65);
        case "M8":
            return mandalaLv === 9 || (mandalaLv >= 66 && mandalaLv <= 73);
        default:
            return false;
    }
}



function search() {
    var criteria = $("#criteria").val().trim().toLowerCase();
    var startDate = $("#startDate").val();
    var endDate = $("#endDate").val();
    var assignee = $("#assignee").val().trim().toLowerCase();
    var selectedMuctieus = [];
    $("input[name='muctieu']:checked").each(function () {
        selectedMuctieus.push($(this).val());
    });

    $("#excelTable tbody tr").each(function () {
        var $row = $(this);
        var targetText = $row.find("select.targetSelect option:selected").text().toLowerCase();
        var rowDeadline = $row.find("input[type='date']").val();
        var rowAssignee = $row.find("input[name$='.Person']").val().toLowerCase();
        var hiddenMandalaLvVal = $row.find("input.hiddenMandalaLv").val();
        var mandalaLv = parseInt(hiddenMandalaLvVal, 10);

        var showRow = true;

        if (criteria && targetText.indexOf(criteria) === -1) {
            showRow = false;
        }
        if (selectedMuctieus.length > 0) {
            var groupMatch = false;
            selectedMuctieus.forEach(function (group) {
                if (isInGroup(mandalaLv, group)) {
                    groupMatch = true;
                }
            });
            if (!groupMatch) {
                showRow = false;
            }
        }
        if (startDate && rowDeadline < startDate) {
            showRow = false;
        }
        if (endDate && rowDeadline > endDate) {
            showRow = false;
        }
        if (assignee && rowAssignee.indexOf(assignee) === -1) {
            showRow = false;
        }

        showRow ? $row.show() : $row.hide();
    });
}

// --- Các chức năng khác ---
// Hàm chỉnh sửa tên Mandala (giữ nguyên logic ban đầu)
function editMandalaName() {

    if (!canEdit) {
        alert("Chế độ xem: Không có quyền chỉnh sửa dữ liệu");
        return;
    }
    var nameSpan = document.getElementById("mandalaNameText");
    var editBtn = document.getElementById("editMandalaNameBtn");
    if (editBtn.textContent.trim() === "Edit") {
        var currentName = nameSpan.textContent.trim();
        nameSpan.innerHTML = '<input type="text" id="mandalaNameInput" value="' + currentName + '" style="font-size:2.5rem; border:none; background:transparent; color:inherit;" />';
        editBtn.textContent = "Save";
    } else {
        var nameInput = document.getElementById("mandalaNameInput");
        var newName = nameInput.value;
        nameSpan.textContent = newName;
        editBtn.textContent = "Edit";
        updateMandalaName(newName);
    }
}

function updateMandalaName(newName) {

    var mandalaId = $("#mandalaId").val();
    $.ajax({
        type: "POST",
        url: "/Table/UpdateMandalaName",
        data: { id: mandalaId, name: newName },
        traditional: true,
        success: function (response) { alert(response.message); },
        error: function (err) { alert("Error updating Mandala Name!"); console.error(err); }
    });
}

function saveData() {
    var mandalaId = $("#mandalaId").val();
    var details = getFormState();
    console.log("Details:", details);
    console.log("Deleted IDs:", deletedIds);
    $.ajax({
        type: "POST",
        url: "/Table/Save",
        data: JSON.stringify({ mandalaId: mandalaId, details: details, deletedIds: deletedIds }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) { alert(response.message); deletedIds = []; window.location.reload(); },
        error: function (err) { alert("Error saving data!"); console.error(err); }
    });
}

function addRow() {
    var rowCount = $("#excelTable tbody tr").length;
    var $newRow = $templateRow.clone();
    $newRow.find("input, select, textarea").each(function () {
        var $input = $(this);
        var name = $input.attr("name");
        if (name) { name = name.replace(/\[\d+\]/, "[" + rowCount + "]"); $input.attr("name", name); }
    });
    $newRow.find("input.hiddenID").val("");
    $newRow.find("select.targetSelect").val("");
    // Khi thêm dòng mới, có thể khởi tạo hiddenMandalaLv rỗng hoặc giá trị mặc định nếu cần
    $newRow.find("input.hiddenMandalaLv").val("");
    $newRow.find("input").not(".hiddenID").each(function () { $(this).val("").prop("checked", false); });
    $("#excelTable tbody").append($newRow);
    pushState();
    autoSortTable();
}

function deleteRows() {
    var $selectedRows = $("#excelTable tbody tr.table-active");
    if ($selectedRows.length > 0) {
        $selectedRows.each(function () {
            var idVal = $(this).find("input.hiddenID").val();
            if (idVal && idVal != 0) { deletedIds.push(idVal); }
        });
        $selectedRows.remove();
        pushState();
        autoSortTable();
    } else { alert("Không có hàng nào được chọn để xóa!"); }
}

// Xử lý click chọn hàng (cho phép chọn nhiều hàng với phím Ctrl)
$("#excelTable tbody").on("click", "tr td:first-child", function (e) {
    if (e.ctrlKey) { $(this).closest("tr").toggleClass("table-active"); }
    else { $("#excelTable tbody tr").removeClass("table-active"); $(this).closest("tr").addClass("table-active"); }
});

$(document).ready(function () {
    // Nếu có dữ liệu mẫu đã load sẵn, lưu mẫu dòng đầu tiên
    if ($("#excelTable tbody tr").length > 0) {
        $templateRow = $("#excelTable tbody tr").first().clone();
        // Cập nhật levelOrder từ dữ liệu mẫu nếu cần
        $("#excelTable tbody tr").each(function () {
            var val = $(this).find("input.hiddenMandalaLv").val();
            // Nếu giá trị tồn tại và chưa có trong mảng levelOrder, thêm vào
            if (val && levelOrder.indexOf(val) === -1) {
                levelOrder.push(val);
            }
        });
    } else {
        $templateRow = $("<tr>" +
            "<td><span class='rowIndex'></span><input type='hidden' name='[0].ID' class='hiddenID' /></td>" +
            "<td>" +
            "<select name='[0].Target' class='targetSelect'>" +
            "<option value=''>Chọn chỉ tiêu</option>" +
            "</select>" +
            "<input type='hidden' name='[0].MandalaLv' class='hiddenMandalaLv' />" +
            "</td>" +
            "<td><input type='date' name='[0].Deadline' class='form-control' /></td>" +
            "<td><input type='checkbox' name='[0].Status' /></td>" +
            "<td><input type='text' name='[0].Action' class='form-control' /></td>" +
            "<td><input type='text' name='[0].Result' class='form-control' /></td>" +
            "<td><input type='text' name='[0].Person' class='form-control' /></td>" +
            "</tr>");
        $("#excelTable tbody").append($templateRow);
    }

    // Nếu các option của select đã khởi tạo, đảm bảo levelOrder chứa các giá trị cần sắp xếp
    $("#excelTable select.targetSelect:first option").each(function () {
        var val = $(this).val();
        if (val && val !== "Chọn chỉ tiêu" && levelOrder.indexOf(val) === -1) {
            levelOrder.push(val);
        }
    });

    // Sau khi load dữ liệu mẫu, gọi sắp xếp lại bảng
    autoSortTable();

    $("#excelForm").on("input change", "input, select, textarea", debounce(function () {
        pushState();
        autoSortTable();
    }, 1000));

    $("#excelTable").on("change", "select.targetSelect", function () {
        var selectedTarget = $(this).val();
        // Cập nhật hiddenMandalaLv khi chọn chỉ tiêu mới
        $(this).closest("tr").find("input.hiddenMandalaLv").val(selectedTarget);
        pushState();
        autoSortTable();
    });
});

// --- Xử lý popup cho Mục Tiêu ---
const muctieuSelector = document.getElementById("muctieuSelector");
const muctieuPopup = document.getElementById("muctieuPopup");
const selectedMuctieuText = document.getElementById("selectedMuctieuText");

muctieuSelector.addEventListener("click", function (event) {
    event.stopPropagation();
    muctieuPopup.classList.toggle("show");
});

document.addEventListener("click", function (event) {
    if (!muctieuSelector.contains(event.target)) { muctieuPopup.classList.remove("show"); }
});

function closeMuctieuPopup() {
    muctieuPopup.classList.remove("show");
    let selectedMuctieus = [];
    document.querySelectorAll("input[name='muctieu']:checked").forEach((checkbox) => { selectedMuctieus.push(checkbox.value); });
    selectedMuctieuText.innerText = selectedMuctieus.length > 0 ? selectedMuctieus.join(", ") : "Chọn Mục Tiêu";
}

// Khi ngày bắt đầu thay đổi, cập nhật thuộc tính min cho ngày kết thúc
document.getElementById("startDate").addEventListener("change", function () {
    const startDateValue = this.value;
    document.getElementById("endDate").min = startDateValue;
    const endDateInput = document.getElementById("endDate");
    if (endDateInput.value && endDateInput.value < startDateValue) { endDateInput.value = ""; }
});

// --- Thêm xử lý: Khi click vào filter-item thì thực hiện chức năng filter ngay lập tức ---
$(document).ready(function () {
    if (!canEdit) {
        // Vô hiệu hóa tất cả các input, select, textarea trong bảng
        $("#excelForm input, #excelForm select, #excelForm textarea").attr("disabled", true);
        // Ngoài ra, ẩn nút "Edit" nếu muốn
        $("#editMandalaNameBtn").hide();
    }

    $(".filter-item").on("click", function (e) {
        if (e.target.tagName.toLowerCase() !== "input") {
            search();
        }
    });
});