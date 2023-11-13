var datatable;
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    datatable = $('#myTable').DataTable({
        "responive": true,
        "ajax": "/Admin/Company/getAll",
        "columns": [
            { data: "name", width: "15%" },
            { data: "phoneNumber" , width:"10%"},
            { data: "street" , width:"15%"},
            { data: "city" , width:"10%"},
            { data: "state" , width:"10%"},
            { data: "postal" , width:"10%"},
            {
                data: "id",
                render: function (data) {
                    return `<div class="w-75 btn-group" role="group">
                                <a href='/Admin/Company/Upsert?id=${data}' class="btn btn-primary mx-2">
                                    <i class="bi bi-pencil-square"></i> Edit
                                </a>
                                <button onClick="Delete('/Admin/Company/Delete/${data}')" class="btn btn-danger mx-2">
                                    <i class="bi bi-trash-fill"></i> Delete
                                </button>
                            </div>`
                },
                width:"20"
            }
        ]
    })
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    datatable.ajax.reload();
                    toastr.success(`${data.message}`)
                }
            })
        }
    })
}