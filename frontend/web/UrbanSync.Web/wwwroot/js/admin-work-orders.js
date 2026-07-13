(function () {
    var PER_PAGE = 8;

    function init() {
        var table = document.getElementById("workOrdersTable");
        if (!table) return;

        var tbody = table.querySelector("tbody");
        var rows = Array.prototype.slice.call(tbody.querySelectorAll("tr"));
        var searchInput = document.getElementById("workOrdersSearch");
        var pagination = document.getElementById("workOrdersPagination");
        var summary = document.getElementById("workOrdersSummary");
        var sortState = { key: null, dir: 1 };
        var page = 1;

        function matchesSearch(row, term) {
            if (!term) return true;
            return row.dataset.search.indexOf(term) !== -1;
        }

        function applySort(visibleRows) {
            if (!sortState.key) return visibleRows;

            return visibleRows.slice().sort(function (a, b) {
                var va = a.dataset[sortState.key] || "";
                var vb = b.dataset[sortState.key] || "";
                if (va < vb) return -1 * sortState.dir;
                if (va > vb) return 1 * sortState.dir;
                return 0;
            });
        }

        function render() {
            var term = (searchInput ? searchInput.value : "").trim().toLowerCase();
            var filtered = rows.filter(function (row) { return matchesSearch(row, term); });
            var sorted = applySort(filtered);
            var totalPages = Math.max(1, Math.ceil(sorted.length / PER_PAGE));
            page = Math.min(page, totalPages);

            rows.forEach(function (row) { row.style.display = "none"; });

            var start = (page - 1) * PER_PAGE;
            sorted.slice(start, start + PER_PAGE).forEach(function (row) {
                row.style.display = "";
                tbody.appendChild(row);
            });

            if (summary) {
                var shown = Math.min(sorted.length, PER_PAGE);
                var from = sorted.length === 0 ? 0 : start + 1;
                var to = Math.min(start + PER_PAGE, sorted.length);
                summary.textContent = from + "–" + to + " de " + sorted.length + " registros";
            }

            renderPagination(totalPages);
        }

        function renderPagination(totalPages) {
            if (!pagination) return;
            pagination.innerHTML = "";

            for (var p = 1; p <= totalPages; p++) {
                var btn = document.createElement("button");
                btn.type = "button";
                btn.className = "btn btn-sm " + (p === page ? "btn-dark" : "btn-outline-dark");
                btn.textContent = String(p);
                btn.addEventListener("click", function (targetPage) {
                    return function () {
                        page = targetPage;
                        render();
                    };
                }(p));
                pagination.appendChild(btn);
            }
        }

        table.querySelectorAll("th[data-sort]").forEach(function (th) {
            th.addEventListener("click", function () {
                var key = th.getAttribute("data-sort");
                if (sortState.key === key) {
                    sortState.dir *= -1;
                } else {
                    sortState.key = key;
                    sortState.dir = 1;
                }

                table.querySelectorAll("th[data-sort] .sort-icon").forEach(function (icon) {
                    icon.textContent = "↕";
                });
                th.querySelector(".sort-icon").textContent = sortState.dir === 1 ? "↑" : "↓";

                page = 1;
                render();
            });
        });

        if (searchInput) {
            searchInput.addEventListener("input", function () {
                page = 1;
                render();
            });
        }

        render();
    }

    document.addEventListener("DOMContentLoaded", init);
})();
