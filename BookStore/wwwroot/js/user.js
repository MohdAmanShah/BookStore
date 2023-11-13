var datatable;
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    datatable = $('#myTable').DataTable({
        "responive": true,
        "ajax": "/Admin/User/getAll",
        "columns": [
            { data: "name", width: "15%" },
            { data: "email", width: "15%" },
            { data: "phoneNumber", width: "15%" },
            { data: "company.name", width: "15%" },
            { data: "role", width: "6%" },
            {
                data: { id : "id", lockoutEnd: "lockoutEnd" },
                render: function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();
                    if (lockout > today) {
                        return `
                        <div class="w-75 btn-group" role="group">
                                <a onclick = "LockUnlock('${ data.id }')" class="btn btn-danger text-white mx-2" style="cursor:pointer; width:150px;">
                                    <i class="bi bi-lock-fill"></i> Locked
                                </a>
                                <a href="/Admin/User/Permission/${data.id}" class="btn btn-danger text-white" style="cursor:pointer; width:150px;">
                                    <i class="bi bi-pencil-square"> Permission</i>
                                </a>
                        </div>
                        `
                    }
                    else {
                        return `
                          <div class="w-75 btn-group" role="group">
                                <a onclick = "LockUnlock('${ data.id }')" class="btn btn-success text-white mx-2" style="cursor:pointer; width:150px;">
                                    <i class="bi bi-unlock-fill"></i> Unlocked
                                </a>
                                <a href="/Admin/User/Permission/${data.id}" class="btn btn-danger text-white" style="cursor:pointer; width:150px;">
                                    <i class="bi bi-pencil-square"> Permission</i>
                                </a>
                        </div>`
                    }
                },
                width: "30%"
            }
        ]
    })
}
function Permisson(Id) {
    
}

//function LockUnlock(id) {
//    Swal.fire({
//        title: 'Are you sure?',
//        text: "You won't be able to revert this!",
//        icon: 'warning',
//        showCancelButton: true,
//        confirmButtonColor: '#3085d6',
//        cancelButtonColor: '#d33',
//        confirmButtonText: 'Yes, delete it!'
//    }).then((result) => {
//        if (result.isConfirmed) {
//            $.ajax({
//                type: 'POST',
//                url: '/Admin/User/Lockout',
//                data: JSON.stringify(id),
//                contentType: "application/json",
//                success: function (data) {
//                    if (data.success) {
//                        datatable.ajax.reload();
//                        toastr.success(`${data.message}`)
//                    }
//                }
//            });
//        }
//    })
//}

function LockUnlock(id) {
    $.ajax({
        type: 'POST',
        url: '/Admin/User/Lockout',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                datatable.ajax.reload();
            }
        }
    })
}