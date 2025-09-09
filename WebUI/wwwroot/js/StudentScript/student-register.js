$(document).ready(function () {
    const universityDisplay = $('#university-display');
    const universityList = $('#university-list');
    const universitySearch = universityList.find('.search-box input');
    const universityItems = universityList.find('li');
    const selectedUniversityName = $('#selected-university-name');
    const form = $('form');

    universityDisplay.on('click', function () {
        universityList.toggle();
    });

    $(document).on('click', function (event) {
        if (!$(event.target).closest('.custom-dropdown-container').length) {
            universityList.hide();
        }
    });

    // Liste elemanına tıklama
    universityItems.on('click', function () {
        const selectedText = $(this).text();
        const selectedName = $(this).data('name');

        universityDisplay.val(selectedText);
        selectedUniversityName.val(selectedName);
        universityList.hide();

        $('#university-display').removeClass('is-invalid');
        $('span[data-valmsg-for="UniversityName"]').text('');
    });

    universitySearch.on('keyup', function () {
        const searchTerm = $(this).val().toLowerCase();
        universityItems.each(function () {
            const text = $(this).text().toLowerCase();
            if (text.includes(searchTerm)) {
                $(this).show();
            } else {
                $(this).hide();
            }
        });
    });

    form.on('submit', function (e) {
        if (!selectedUniversityName.val()) {
            e.preventDefault();
            $('#university-display').addClass('is-invalid');
            $('span[data-valmsg-for="UniversityName"]').text('Üniversite seçimi zorunludur.');
        }
    });

});