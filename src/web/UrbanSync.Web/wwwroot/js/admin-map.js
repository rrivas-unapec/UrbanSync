(function () {
    function dotIcon(color) {
        return L.divIcon({
            className: "",
            html: '<span style="display:block;width:14px;height:14px;border-radius:50%;background:' + color + ';border:2px solid #fff;box-shadow:0 0 0 1px rgba(0,0,0,.25);"></span>',
            iconSize: [14, 14],
            iconAnchor: [7, 7]
        });
    }

    function init() {
        var dataEl = document.getElementById("mapPointsData");
        var mapEl = document.getElementById("incidentMap");
        if (!dataEl || !mapEl) return;

        var points = JSON.parse(dataEl.textContent);
        if (points.length === 0) return;

        var map = L.map(mapEl).setView([18.4861, -69.9312], 12);

        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            maxZoom: 19,
            attribution: "&copy; OpenStreetMap contributors"
        }).addTo(map);

        var activeTipos = new Set(points.map(function (p) { return p.tipoIncidencia; }));
        var clusteringOn = true;
        var heatmapOn = false;
        var clusterGroup = L.markerClusterGroup();
        var plainGroup = L.layerGroup();
        var heatLayer = null;

        var markers = points.map(function (p) {
            var marker = L.marker([p.lat, p.lng], { icon: dotIcon(p.color) }).bindPopup(
                "<strong>" + p.codigoCaso + "</strong><br>" +
                p.tipoIncidencia + " · " + p.prioridad + "<br>" +
                p.direccion
            );
            marker.tipoIncidencia = p.tipoIncidencia;
            return marker;
        });

        function rebuildMarkers() {
            clusterGroup.clearLayers();
            plainGroup.clearLayers();
            map.removeLayer(clusterGroup);
            map.removeLayer(plainGroup);

            var visible = markers.filter(function (m) { return activeTipos.has(m.tipoIncidencia); });
            var target = clusteringOn ? clusterGroup : plainGroup;
            visible.forEach(function (m) { target.addLayer(m); });
            map.addLayer(target);
        }

        function rebuildHeat() {
            if (heatLayer) {
                map.removeLayer(heatLayer);
                heatLayer = null;
            }
            if (!heatmapOn) return;

            var visiblePoints = points
                .filter(function (p) { return activeTipos.has(p.tipoIncidencia); })
                .map(function (p) { return [p.lat, p.lng, 0.6]; });
            heatLayer = L.heatLayer(visiblePoints, { radius: 25 }).addTo(map);
        }

        function updateSubtitle() {
            var subtitle = document.getElementById("mapSubtitle");
            if (!subtitle) return;
            var n = activeTipos.size;
            subtitle.textContent = n + (n === 1 ? " capa activa" : " capas activas");
        }

        document.querySelectorAll("[data-map-filter]").forEach(function (btn) {
            btn.addEventListener("click", function () {
                var tipo = btn.getAttribute("data-map-filter");
                if (activeTipos.has(tipo)) {
                    activeTipos.delete(tipo);
                    btn.classList.remove("active");
                } else {
                    activeTipos.add(tipo);
                    btn.classList.add("active");
                }
                rebuildMarkers();
                rebuildHeat();
                updateSubtitle();
            });
        });

        var clusterBtn = document.getElementById("mapClusterToggle");
        if (clusterBtn) {
            clusterBtn.addEventListener("click", function () {
                clusteringOn = !clusteringOn;
                clusterBtn.classList.toggle("active", clusteringOn);
                rebuildMarkers();
            });
        }

        var heatBtn = document.getElementById("mapHeatToggle");
        if (heatBtn) {
            heatBtn.addEventListener("click", function () {
                heatmapOn = !heatmapOn;
                heatBtn.classList.toggle("active", heatmapOn);
                rebuildHeat();
            });
        }

        rebuildMarkers();
        updateSubtitle();
    }

    document.addEventListener("DOMContentLoaded", init);
})();
