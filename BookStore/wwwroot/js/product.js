﻿var datatable;
$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    datatable = $('#myTable').DataTable({
        "responive": true,
        "ajax": "/Admin/Product/getAll",
        "columns": [
            { data: "title", width: "15%" },
            { data: "author" , width:"15%"},
            { data: "isbn" , width:"15%"},
            { data: "listPrice" , width:"15%"},
            { data: "category.name" , width:"15%"},
            {
                data: "productId",
                render: function (data) {
                    return `<div class="w-75 btn-group" role="group">
                                <a href='/Admin/Product/Upsert?id=${data}' class="btn btn-primary mx-2">
                                    <i class="bi bi-pencil-square"></i> Edit
                                </a>
                                <button onClick="Delete('/Admin/Product/Delete/${data}')" class="btn btn-danger mx-2">
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