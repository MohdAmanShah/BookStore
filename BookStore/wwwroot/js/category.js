var datatable;
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    datatable = $('#myTable').DataTable({
        "responive": true,
        "ajax": "/Admin/Category/getAll",
        "columns": [
            { data: "name", width: "25%" },
            {
                data: "id",
                render: function (data) {
                    return `<div class="w-75 btn-group" role="group">
                                <a href='/Admin/Category/Upsert?id=${data}' class="btn btn-primary mx-2">
                                    <i class="bi bi-pencil-square"></i> Edit
                                </a>
                                <a onClick="Delete('/Admin/Category/Delete/${data}')" class="btn btn-danger mx-2">
                                    <i class="bi bi-trash-fill"></i> Delete
                                </a>
                            </div>`
                },
                width:"30%"
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