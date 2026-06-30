// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
function parseDate(dateString) {
    var parts = dateString.split("/"); // Split the string into an array of day, month, and year
    if (parts.length === 3) {
        var day = parseInt(parts[0], 10);
        var month = parseInt(parts[1], 10) - 1; // JavaScript months are 0-based (0 = January, 11 = December)
        var year = parseInt(parts[2], 10);

        // Create a new Date object with the parsed components
        var date = new Date(year, month, day);

        if (isNaN(date)) {
            return null
        } else {
            return date;
        }
    } else {
        return null;
    }
}

function debounce(func, wait) {
    let timeout;

    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };

        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

function getDeleteModal(id, type, name, actionLink, callBack) {
    return `
        <div id="confirmDelete-${id}" tabindex="-1" class="fixed top-0 left-0 right-0 z-50 hidden p-4 overflow-x-hidden overflow-y-auto md:inset-0 h-[calc(100%-1rem)] max-h-full">
            <div class="relative w-full max-w-md max-h-full">
                <div class="relative bg-white rounded-lg shadow dark:bg-gray-700">
                    <button onclick="toggleDeleteModal('${id}', false)" type="button" class="absolute top-3 right-2.5 text-gray-400 bg-transparent hover:bg-gray-200 hover:text-gray-900 rounded-lg text-sm w-8 h-8 ml-auto inline-flex justify-center items-center dark:hover:bg-gray-600 dark:hover:text-white" data-modal-hide="confirmDelete-${id}">
                        <svg class="w-3 h-3" aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 14 14">
                            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m1 1 6 6m0 0 6 6M7 7l6-6M7 7l-6 6"/>
                        </svg>
                        <span class="sr-only">Close modal</span>
                    </button>
                    <div class="p-6 text-center">
                        <svg class="mx-auto mb-4 text-gray-400 w-12 h-12 dark:text-gray-200" aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 20 20">
                            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 11V6m0 8h.01M19 10a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z"/>
                        </svg>
                        <h3 class="mb-5 text-lg font-normal text-gray-500 dark:text-gray-400 text-wrap">Bạn có chắc chắn muốn xóa ${type} <strong>${name}</strong> ra khỏi hệ thống </h3>
                        <div class="w-full flex justify-center items-center">
                            <button onclick="deleteAction('${actionLink}', '${id}', ${callBack})" data-modal-hide="confirmDelete-${id}" type="submit" class="text-white bg-red-600 hover:bg-red-800 focus:ring-4 focus:outline-none focus:ring-red-300 dark:focus:ring-red-800 font-medium rounded-lg text-sm inline-flex items-center px-5 py-2.5 text-center mr-2">
                                Đồng ý
                            </button>
                            <button onclick="toggleDeleteModal('${id}', false)" data-modal-hide="confirmDelete-${id}" type="button" class="text-gray-500 bg-white hover:bg-gray-100 focus:ring-4 focus:outline-none focus:ring-gray-200 rounded-lg border border-gray-200 text-sm font-medium px-5 py-2.5 hover:text-gray-900 focus:z-10 dark:bg-gray-700 dark:text-gray-300 dark:border-gray-500 dark:hover:text-white dark:hover:bg-gray-600 dark:focus:ring-gray-600">Hủy</button>
                        </div>
                    </div>
                </div>
            </div>
        </div> 
    `
}

function deleteAction(url, id, callBack) {
    $.ajax({
        url: url + '/' + id,
        type: 'POST',
        processData: false,
        contentType: false,
        success: function (result) {
            callBack(true)
        },
        error: function (xhr, textStatus, errorThrown) {
            callBack(false)
        }
    });
}

function toggleDeleteModal(id, isOpen = true) {
    const $targetEl = document.getElementById(`confirmDelete-${id}`);

    const instanceOptions = {
        id: `confirmDelete-${id}`,
        override: true
    };
    const modal = new Modal($targetEl, {closable: true}, instanceOptions);
    if (isOpen) {
        modal.show()
    } else {
        $('[modal-backdrop]').remove()
        modal.hide()
    }
}

function isPositiveInteger(str) {
    const pattern = /^\d+$/;
    return pattern.test(str) && parseInt(str) >= 0;
}

function compareTimes(time1, time2) {
    // Split the times into hours and minutes
    const [hours1, minutes1] = time1.split(":").map(Number);
    const [hours2, minutes2] = time2.split(":").map(Number);

    // Create Date objects for each time, with a fixed date
    const date1 = new Date(2000, 0, 1, hours1, minutes1); // Year, Month, Day, Hours, Minutes
    const date2 = new Date(2000, 0, 1, hours2, minutes2);

    // Compare the Date objects
    if (date1 < date2) {
        return -1; // time1 is less than time2
    } else if (date1 > date2) {
        return 1; // time1 is greater than time2
    } else {
        return 0; // time1 is equal to time2
    }
}

function showToast(message, isSuccess = true) {
    Toastify({
        text: message,
        duration: 4000,
        close: true,
        gravity: "top", // `top` or `bottom`
        position: "right", // `left`, `center` or `right`
        stopOnFocus: true, // Prevents dismissing of toast on hover
        style: {
            background: "white",
            color: isSuccess ? 'green' : 'red'
        },
    }).showToast();
}

function formatDate(date, withTime = true) {
    var day = date.getDate();
    var month = date.getMonth() + 1; // Months are zero indexed
    var year = date.getFullYear();
    var hours = date.getHours();
    var minutes = date.getMinutes();

    // Add leading zeros to day, month, hours, and minutes if needed
    day = (day < 10) ? '0' + day : day;
    month = (month < 10) ? '0' + month : month;
    hours = (hours < 10) ? '0' + hours : hours;
    minutes = (minutes < 10) ? '0' + minutes : minutes;

    return day + '/' + month + '/' + year + (withTime ? ' ' + hours + ':' + minutes : '');
}

// Write your JavaScript code.
