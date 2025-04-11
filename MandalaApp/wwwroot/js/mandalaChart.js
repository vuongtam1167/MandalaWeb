// Lấy dữ liệu từ model (được truyền từ view)
let placeholders3 = JSON.parse(document.getElementById("placeholders3Data").textContent);
let placeholders9 = JSON.parse(document.getElementById("placeholders9Data").textContent);
let isGrid3x3 = true; // Mặc định sử dụng grid 3x3

// Các mảng thứ tự (orderedPositions) cho grid 3x3 và 9x9
const orderedPositions3x3 = [7, 4, 8, 3, 1, 5, 6, 2, 9];
const orderedPositions9 = [
    55, 52, 56, 31, 28, 32, 63, 60, 64,
    51, 7, 53, 27, 4, 29, 59, 8, 61,
    54, 50, 57, 30, 26, 33, 62, 58, 65,
    23, 20, 24, 7, 4, 8, 39, 36, 40,
    19, 3, 21, 3, 1, 5, 35, 5, 37,
    22, 18, 25, 6, 2, 9, 38, 34, 41,
    47, 44, 48, 15, 12, 16, 71, 68, 72,
    43, 6, 45, 11, 2, 13, 67, 9, 69,
    46, 42, 49, 14, 10, 17, 70, 66, 73
];

// Định nghĩa unlockOrder cho grid 3x3 (0-based)
const unlockOrder3x3 = [4, 7, 3, 1, 5, 6, 0, 2, 8];

// Định nghĩa unlockOrder cho grid 9x9 (chuyển từ 1-based về 0-based)
const unlockOrder9x9_part1 = [40, 49, 39, 31, 41, 48, 30, 32, 50];
const unlockOrder9x9_part2 = [67, 76, 66, 58, 68, 75, 57, 59, 77];
const unlockOrder9x9_part3 = [37, 46, 36, 28, 38, 45, 27, 29, 47];
const unlockOrder9x9_part4 = [13, 22, 12, 4, 14, 21, 3, 5, 23];
const unlockOrder9x9_part5 = [43, 52, 42, 34, 44, 51, 33, 35, 53];
const unlockOrder9x9_part6 = [64, 73, 63, 55, 65, 72, 54, 56, 74];
const unlockOrder9x9_part7 = [10, 19, 9, 1, 11, 18, 0, 2, 20];
const unlockOrder9x9_part8 = [16, 25, 15, 7, 17, 24, 6, 8, 26];
const unlockOrder9x9_part9 = [70, 79, 69, 61, 71, 78, 60, 62, 80];
const unlockOrder9x9 = [
    ...unlockOrder9x9_part1,
    ...unlockOrder9x9_part2,
    ...unlockOrder9x9_part3,
    ...unlockOrder9x9_part4,
    ...unlockOrder9x9_part5,
    ...unlockOrder9x9_part6,
    ...unlockOrder9x9_part7,
    ...unlockOrder9x9_part8,
    ...unlockOrder9x9_part9
];

// Ánh xạ các ô đồng bộ (0-based)
const syncCells = {
    49: [67],
    67: [49],
    39: [37],
    37: [39],
    31: [13],
    13: [31],
    41: [43],
    43: [41],
    48: [64],
    64: [48],
    30: [10],
    10: [30],
    32: [16],
    16: [32],
    50: [70],
    70: [50]
};

// ---------------- Undo/Redo ----------------
let undoHistory = [];
let redoHistory = [];

// Khi lưu trạng thái, lưu luôn cả giá trị của isGrid3x3 để toàn bộ trạng thái được lưu trữ
function pushState() {
    undoHistory.push({
        placeholders3: placeholders3.slice(),
        placeholders9: placeholders9.slice(),
        isGrid3x3: isGrid3x3
    });
    // Sau khi push trạng thái mới, xóa redoHistory
    redoHistory = [];
}

function undo() {
    if (undoHistory.length > 0) {
        redoHistory.push({
            placeholders3: placeholders3.slice(),
            placeholders9: placeholders9.slice(),
            isGrid3x3: isGrid3x3
        });
        let state = undoHistory.pop();
        placeholders3 = state.placeholders3.slice();
        placeholders9 = state.placeholders9.slice();
        isGrid3x3 = state.isGrid3x3;
        generateGrid();
    }
}

function redo() {
    if (redoHistory.length > 0) {
        undoHistory.push({
            placeholders3: placeholders3.slice(),
            placeholders9: placeholders9.slice(),
            isGrid3x3: isGrid3x3
        });
        let state = redoHistory.pop();
        placeholders3 = state.placeholders3.slice();
        placeholders9 = state.placeholders9.slice();
        isGrid3x3 = state.isGrid3x3;
        generateGrid();
    }
}
// ---------------- End Undo/Redo ----------------

// Hàm đồng bộ dữ liệu giữa 2 grid dựa trên matching mandalaLv
function syncData(sourcePlaceholders, sourceOrdered, targetPlaceholders, targetOrdered) {
    for (let i = 0; i < sourcePlaceholders.length; i++) {
        let value = sourcePlaceholders[i];
        if (value && value.trim() !== "") {
            let sourceLevel = sourceOrdered[i];
            for (let j = 0; j < targetOrdered.length; j++) {
                if (targetOrdered[j] === sourceLevel) {
                    targetPlaceholders[j] = value;
                }
            }
        }
    }
}

function generateGrid() {
    const size = isGrid3x3 ? 3 : 9;
    const container = document.getElementById('mandala-grid');
    container.innerHTML = '';
    container.style.gridTemplateRows = `repeat(${size}, 1fr)`;
    container.style.gridTemplateColumns = `repeat(${size}, 1fr)`;
    container.style.width = `${size * 120 + (size - 1) * 10}px`;
    container.style.height = `${size * 120 + (size - 1) * 10}px`;

    const currentOrderedPositions = isGrid3x3 ? orderedPositions3x3 : orderedPositions9;
    let data = isGrid3x3 ? placeholders3 : placeholders9;

    for (let index = 0; index < size * size; index++) {
        const cell = document.createElement('div');
        cell.className = 'cell';
        let cellNumber = currentOrderedPositions[index];
        if (cellNumber === 1) {
            cell.classList.add('red');
        } else if (cellNumber >= 2 && cellNumber <= 9) {
            cell.classList.add('blue');
        }
        cell.dataset.mandalaLv = cellNumber;
        cell.dataset.index = index;
        const textarea = document.createElement('textarea');
        textarea.value = data[index] || "";
        textarea.dataset.index = index;
        textarea.dataset.mandalaLv = cellNumber;
        textarea.setAttribute("rows", "3");

        // Nếu không có quyền chỉnh sửa, disable tất cả ô
        if (!canEdit) {
            textarea.disabled = true;
        }

        cell.appendChild(textarea);
        container.appendChild(cell);
    }
    updateUnlocking();
}

// Cập nhật unlocking và đồng bộ các ô theo thứ tự unlocking
function updateUnlocking() {
    if (!canEdit) return;
    const unlockOrder = isGrid3x3 ? unlockOrder3x3 : unlockOrder9x9;
    for (let i = 0; i < unlockOrder.length; i++) {
        let cell = document.querySelector(`textarea[data-index="${unlockOrder[i]}"]`);
        if (!cell) continue;
        if (i === 0) {
            cell.disabled = false;
        } else {
            let prevCell = document.querySelector(`textarea[data-index="${unlockOrder[i - 1]}"]`);
            cell.disabled = !(prevCell && prevCell.value.trim() !== "");
        }
    }
    for (let key in syncCells) {
        let cell = document.querySelector(`textarea[data-index="${key}"]`);
        if (cell && !cell.disabled && cell.value.trim() !== "") {
            syncCells[key].forEach(function (otherIdx) {
                let otherCell = document.querySelector(`textarea[data-index="${otherIdx}"]`);
                if (otherCell) {
                    otherCell.disabled = false;
                }
            });
        }
    }
}

// Toggle grid: đồng bộ dữ liệu giữa 2 grid, lưu trạng thái và vẽ lại grid
function toggleGrid() {
    if (isGrid3x3) {
        syncData(placeholders3, orderedPositions3x3, placeholders9, orderedPositions9);
    } else {
        syncData(placeholders9, orderedPositions9, placeholders3, orderedPositions3x3);
    }
    isGrid3x3 = !isGrid3x3;
    pushState();
    generateGrid();
}

function handleSave() {
    if (!canEdit) return;
    let gridCells = document.querySelectorAll('.mandala-grid .cell textarea');
    let data = Array.from(gridCells).map(cell => cell.value.trim() === "" ? null : cell.value);
    if (!isGrid3x3 && data.length !== 81) {
        alert("Lỗi! Lưới 9x9 chưa đủ 81 ô.");
        return;
    }
    let mandalaId = document.getElementById("mandalaId").value;
    console.log("mandalaId:", mandalaId);
    console.log("data:", data);
    console.log("isGrid3x3:", isGrid3x3);
    $.ajax({
        type: "POST",
        url: "/Chart/SaveMandalaTargets",
        data: { mandalaId: mandalaId, data: data },
        traditional: true,
        success: function (response) {
            alert(response.message);
        },
        error: function (err) {
            alert("Có lỗi xảy ra khi lưu dữ liệu!");
            console.error(err);
        }
    });
}

function editMandalaName() {
    if (!canEdit) {
        alert("Chế độ xem: Không có quyền chỉnh sửa dữ liệu");
        return;
    }
    let nameSpan = document.getElementById("mandalaNameText");
    let editBtn = document.getElementById("editMandalaNameBtn");
    if (editBtn.textContent.trim() === "Edit") {
        let currentName = nameSpan.textContent.trim();
        nameSpan.innerHTML = '<input type="text" id="mandalaNameInput" value="' + currentName + '" style="font-size:2.5rem; border:none; background:transparent; color:inherit;" />';
        editBtn.textContent = "Save";
    } else {
        let nameInput = document.getElementById("mandalaNameInput");
        let newName = nameInput.value;
        nameSpan.textContent = newName;
        editBtn.textContent = "Edit";
        updateMandalaName(newName);
    }
}

function updateMandalaName(newName) {
    if (!canEdit) return;
    let mandalaId = document.getElementById("mandalaId").value;
    $.ajax({
        type: "POST",
        url: "/Chart/UpdateMandalaName",
        data: { id: mandalaId, name: newName },
        traditional: true,
        success: function (response) {
            alert(response.message);
        },
        error: function (err) {
            alert("Error updating Mandala Name!");
            console.error(err);
        }
    });
}

function viewDetail() {
    let mandalaId = document.getElementById("mandalaId").value;
    window.location.href = "/Table/Table?mandalaId=" + mandalaId;
}

function upgradeClass() {
    let mandalaId = document.getElementById("mandalaId").value;
    if (confirm("Are you sure you want to upgrade to a 9x9 Mandala? This action cannot be undone!")) {
        $.ajax({
            type: "POST",
            url: "/Chart/UpgradeMandalaClass",
            data: { mandalaId: mandalaId },
            success: function (response) {
                alert(response.message);
                if (response.success) {
                    window.location.reload();
                }
            },
            error: function (err) {
                alert("Error upgrading Mandala!");
                console.error(err);
            }
        });
    }
}

// Sử dụng event delegation: lưu trạng thái sau mỗi lần nhập (mỗi ký tự)
// Lưu trạng thái ngay sau khi người dùng gõ (mỗi ký tự)
document.getElementById("mandala-grid").addEventListener("input", function (e) {
    if (e.target && e.target.tagName === "TEXTAREA") {
        let idx = parseInt(e.target.dataset.index);
        let newValue = e.target.value;
        if (isGrid3x3) {
            placeholders3[idx] = newValue;
        } else {
            placeholders9[idx] = newValue;
        }
        if (syncCells.hasOwnProperty(idx)) {
            syncCells[idx].forEach(function (otherIdx) {
                let otherCell = document.querySelector(`textarea[data-index="${otherIdx}"]`);
                if (otherCell) {
                    otherCell.value = newValue;
                    if (isGrid3x3) {
                        placeholders3[otherIdx] = newValue;
                    } else {
                        placeholders9[otherIdx] = newValue;
                    }
                }
            });
        }
        updateUnlocking();
        // Lưu trạng thái sau mỗi ký tự
        pushState();
    }
});

// Khởi tạo grid và lưu trạng thái ban đầu
generateGrid();
pushState();
