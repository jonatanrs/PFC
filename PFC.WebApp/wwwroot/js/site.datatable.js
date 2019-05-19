$(function () {

    $("[data-table]").each(function () {
        let dataTableContainer = $(this);
        let dataTable = $(this).find("table");
        let template = _.template(dataTableContainer.find("script[type='text/template']").html());

        UpdatePaginador = function (index, length, total) {

            let paginasTotales = index + 1;
            let paginas = [index + 1];

            if (total > 0) {
                paginasTotales = Math.ceil(total / length);
                paginas = calculaPaginador(index + 1, paginasTotales, 5);
            }

            var paginador = dataTableContainer.find("[data-role='paginador']");

            paginador.html("");

            var botonPrincipio = $('<li><a href="#" data-page="0"><i class="glyphicon glyphicon-fast-backward"></i></a></li>');
            var botonAnterior = $('<li><a href="#" data-page="' + (index - 1) + '"><i class="glyphicon glyphicon-step-backward"></i></a></li>');
            if (paginas.length == 0 || paginas[0] == 1) {
                botonPrincipio.addClass("disabled");
                botonAnterior.addClass("disabled");
            }
            paginador.append(botonPrincipio);
            paginador.append(botonAnterior);

            paginas.forEach(function (element) {
                var li = $('<li><a href="#" data-page="' + (element - 1) + '">' + element + '</a></li>')
                if (element == index + 1)
                    li.addClass("active");

                paginador.append(li);
            });

            var botonSiguinte = $('<li><a href="#" data-page="' + (index + 1) + '"><i class="glyphicon glyphicon-step-forward"></i></a></li>');
            var botonUltimo = $('<li><a href="#" data-page="' + (paginasTotales - 1) + '"><i class="glyphicon glyphicon-fast-forward"></i></a></li>');
            if (paginas.length == 0 || paginas[paginas.length - 1] == paginasTotales) {
                if(total > 0)
                    botonSiguinte.addClass("disabled");
                botonUltimo.addClass("disabled");
            }
            paginador.append(botonSiguinte);
            paginador.append(botonUltimo);
        };

        // Calcula un rango de páginas definido por tamañoPAginador, manteniendo centrada la página actual
        let calculaPaginador = function (paginaActual, paginasTotales, tamañoPaginador) {
            var result = [];

            var numerosPorLados = Math.trunc((tamañoPaginador - 1) / 2);

            var sobrantesIzquierda = Math.max(numerosPorLados - paginaActual + 1, 0);
            var sobrantesDerecha = Math.max(paginaActual - paginasTotales + numerosPorLados, 0);
            var primero = Math.max(1, (paginaActual - numerosPorLados) - sobrantesDerecha);
            var ultimo = Math.min(paginasTotales, (paginaActual + numerosPorLados) + sobrantesIzquierda);

            for (i = primero; i <= ultimo; i++) {
                result.push(i);
            }

            return result;
        };

        let loadData = function (index, length, data) {

            var dataTableBody = dataTable.find("tbody[data-role='resultados']");
            var dataTableLoading = dataTable.find("tbody[data-role='info'] .progress");
            var dataTableError = dataTable.find("tbody[data-role='info'] .alert");

            dataTableBody.hide();
            dataTableError.hide();
            dataTableLoading.show();
            dataTable.find("tbody[data-role='info']").show();

            $.ajax(dataTableContainer.data("tableListUrl") + "?index=" + index + "&length=" + length + (dataTableContainer.data("orderBy") ? "&orderProperty=" + dataTableContainer.data("orderBy") + "&inverse=" + (dataTableContainer.data("inverse") ? "True" : "False") : ""), {
                method: "POST",
                contentType: "application/json",
                data: data
            }).done(function (data) {
                if (data.Success) {
                    dataTableBody.html("");
                    data.Elements.forEach(function (element) {
                        dataTableBody.append(template(element));
                    });
                    UpdatePaginador(index, length, data.Total);
                    dataTable.find("tbody[data-role='info']").hide();
                    dataTableBody.show();
                } else {
                    dataTableError.find("[data-role='error-messages']").html("").append("<li>" + data.ErrorMessage + "</li>")
                    dataTableLoading.hide();
                    dataTableError.show();
                }
            }).fail(function (data) {
                dataTableLoading.hide();
                dataTableError.show();
            });
        };

        var dataFiltros = function () {
            var data = dataTable.find("input").map(function () { return { field: $(this).attr("name"), value: $(this).val(), operator: $(this).data("defaultOperator") }; }).toArray();
            return JSON.stringify(data);
        }

        dataTable.on("change", "input", function () {
            loadData(0, dataTableContainer.find("[data-role='elementos-por-pagina']").val(), dataFiltros());
        });

        dataTableContainer.on("change", "[data-role='elementos-por-pagina']", function () {
            loadData(0, dataTableContainer.find("[data-role='elementos-por-pagina']").val(), dataFiltros());
        });

        dataTableContainer.on("click", "[data-role='paginador'] :not(.disabled) a", function () {
            let $this = $(this);
            $this.closest("[data-role='paginador']").find("li").removeClass("active");
            $this.closest("li").addClass("active");
            loadData($this.data("page"), dataTableContainer.find("[data-role='elementos-por-pagina']").val(), dataFiltros());
        });

        dataTable.on("click", "thead label", function () {
            let $this = $(this);
            let icon = $this.find("i[data-role='order-icon']");

            dataTableContainer.data("orderBy", $this.closest("th").find("input").attr("name"));
            dataTableContainer.data("inverse", false);

            if (icon.hasClass("glyphicon-sort-by-attributes")) {
                // Togle downup
                icon.removeClass("glyphicon-sort-by-attributes");
                icon.addClass("glyphicon-sort-by-attributes-alt");
                dataTableContainer.data("inverse", true);
            } else if (icon.hasClass("glyphicon-sort-by-attributes-alt")) {
                // Togle upoDown
                icon.removeClass("glyphicon-sort-by-attributes-alt");
                icon.addClass("glyphicon-sort-by-attributes");
            } else {
                // Cahnege campo orden
                $this.closest("thead").find("label i")
                    .removeClass("glyphicon-sort-by-attributes glyphicon-sort-by-attributes-alt");
                icon.addClass("glyphicon-sort-by-attributes");
            }
            loadData(0, dataTableContainer.find("[data-role='elementos-por-pagina']").val(), dataFiltros());
        });

        dataTable.on("click", "tbody[data-role='resultados'] td", function () {
            let $this = $(this);
            $(dataTable.find("thead input")[$this.closest("tr").find("td").index($this)]).val($this.text()).trigger("change");
        });

        dataTable.on("click", "thead button", function () {
            $(this).closest(".input-group").find("input").val("").trigger("change");
        });

        dataTable.on("click", "[data-role='reload']", function () {
            dataTable.find("input").val("")
            loadData(0, dataTableContainer.find("[data-role='elementos-por-pagina']").val());
        });

        loadData(0, dataTableContainer.find("[data-role='elementos-por-pagina']").val());
    });

});