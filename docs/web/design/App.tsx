import { useState, useRef, useCallback } from "react";
import {
  MapPin, AlertTriangle, CheckCircle, Clock, TrendingUp, TrendingDown,
  Camera, Navigation, WifiOff, Bell, Search, Filter, ChevronDown,
  ChevronUp, ChevronLeft, ChevronRight, Award, Package, Layers,
  BarChart2, Map, List, FileText, Home, User, X, Plus, Check,
  AlertCircle, Info, Wrench, Star, Truck, Users, Settings,
  RefreshCw, Download, Sun, Type, MoreVertical, LogOut, Route,
  Shield, Wind, Phone, Eye, Activity,
} from "lucide-react";
import {
  AreaChart, Area, BarChart, Bar, XAxis, YAxis, CartesianGrid,
  Tooltip, ResponsiveContainer,
} from "recharts";

// ─── Design tokens ─────────────────────────────────────────────────────────
const B = "#0057B8"; // Institutional blue
const T = "#00A676"; // Sustainability teal
const Y = "#FFB800"; // Alert yellow
const R = "#E4572E"; // Critical red
const G900 = "#1A1A1A";
const G500 = "#717182";
const G200 = "#E0E2E8";
const G50 = "#F5F6F8";

// ─── Priority / Status config ──────────────────────────────────────────────
const PRIORITY = {
  critical: { label: "Crítica",  bg: "#FEE9E3", fg: R,         dot: R },
  high:     { label: "Alta",     bg: "#FFF3E0", fg: "#C65D00", dot: "#F59E0B" },
  medium:   { label: "Media",    bg: "#FFFBEB", fg: "#856404", dot: Y },
  low:      { label: "Baja",     bg: "#F0FDF4", fg: "#166534", dot: "#22C55E" },
} as const;

const STATUS = {
  pending:      { label: "Pendiente",    bg: "#F3F4F6", fg: "#374151" },
  assigned:     { label: "Asignada",     bg: "#EFF6FF", fg: B },
  "in-progress":{ label: "En Progreso",  bg: "#FFFBEB", fg: "#856404" },
  resolved:     { label: "Resuelta",     bg: "#F0FDF4", fg: "#166534" },
  received:     { label: "Recibido",     bg: "#F3F4F6", fg: "#374151" },
  verified:     { label: "Verificado",   bg: "#EFF6FF", fg: B },
  escalated:    { label: "Escalado",     bg: "#FEE9E3", fg: R },
} as const;

const ISSUE_TYPES = [
  { id: "pothole",     label: "Bache",     emoji: "🕳️", color: "#7C3AED" },
  { id: "streetlight", label: "Luminaria", emoji: "💡", color: Y },
  { id: "traffic",     label: "Semáforo",  emoji: "🚦", color: R },
  { id: "park",        label: "Parque",    emoji: "🌳", color: T },
  { id: "sidewalk",    label: "Acera",     emoji: "🧱", color: G500 },
] as const;

type Role = "citizen" | "technician" | "admin";
type PriorityKey = keyof typeof PRIORITY;
type StatusKey = keyof typeof STATUS;

// ─── Mock data ─────────────────────────────────────────────────────────────
const WORK_ORDERS = [
  { id: "WO-2847", type: "pothole",     title: "Bache crítico Av. Winston Churchill",    address: "Av. Winston Churchill esq. Haim López Penha, Naco",     priority: "critical" as PriorityKey, status: "in-progress" as StatusKey, assignee: "Carlos Méndez",  dueDate: "2026-07-10", district: "Naco",        created: "hace 2 días" },
  { id: "WO-2843", type: "streetlight", title: "Luminaria apagada Calle El Conde",       address: "Calle El Conde #124, Ciudad Colonial",                  priority: "high"     as PriorityKey, status: "assigned"    as StatusKey, assignee: "Ana Rodríguez", dueDate: "2026-07-11", district: "C. Colonial", created: "hace 3 días" },
  { id: "WO-2839", type: "traffic",     title: "Semáforo dañado Av. 27 de Febrero",      address: "Av. 27 de Febrero esq. Av. Tiradentes, Gazcue",         priority: "high"     as PriorityKey, status: "pending"     as StatusKey, assignee: null,            dueDate: "2026-07-12", district: "Gazcue",      created: "hace 4 días" },
  { id: "WO-2835", type: "sidewalk",    title: "Acera deteriorada Av. Abraham Lincoln",  address: "Av. Abraham Lincoln #89, Piantini",                    priority: "medium"   as PriorityKey, status: "in-progress" as StatusKey, assignee: "José García",   dueDate: "2026-07-15", district: "Piantini",    created: "hace 5 días" },
  { id: "WO-2830", type: "park",        title: "Mantenimiento Parque Mirador Sur",        address: "Parque Mirador del Sur, sector Este, Bella Vista",      priority: "low"      as PriorityKey, status: "resolved"    as StatusKey, assignee: "María Santos",  dueDate: "2026-07-09", district: "Bella Vista", created: "hace 6 días" },
  { id: "WO-2826", type: "pothole",     title: "Baches múltiples Av. Máximo Gómez",      address: "Av. Máximo Gómez, tramo km 3-5",                       priority: "critical" as PriorityKey, status: "pending"     as StatusKey, assignee: null,            dueDate: "2026-07-10", district: "Gazcue",      created: "hace 7 días" },
];

const ASSETS = [
  { id: "ACT-0124", type: "Semáforo",     location: "Av. JFK esq. Av. Tiradentes",          status: "operational", lastMaint: "2026-06-15", nextMaint: "2026-09-15" },
  { id: "ACT-0231", type: "Luminaria",    location: "Calle El Conde #124",                  status: "operational", lastMaint: "2026-05-20", nextMaint: "2026-08-20" },
  { id: "ACT-0387", type: "Boca de pozo", location: "Av. Abraham Lincoln #45",              status: "maintenance", lastMaint: "2026-07-01", nextMaint: "2026-07-10" },
  { id: "ACT-0442", type: "Señal vial",   location: "Autopista Duarte km 12",               status: "critical",    lastMaint: "2026-04-10", nextMaint: "2026-07-10" },
  { id: "ACT-0518", type: "Semáforo",     location: "Av. Winston Churchill esq. 27 Feb.",   status: "operational", lastMaint: "2026-06-30", nextMaint: "2026-09-30" },
  { id: "ACT-0624", type: "Luminaria",    location: "Parque Mirador del Sur, entrada N",    status: "offline",     lastMaint: "2026-05-01", nextMaint: "2026-07-08" },
];

const KPIS = [
  { label: "Activos Totales",       value: "12,847", unit: "",           trend: "up"   as const, change: "+3.2%",  color: B,  Icon: Package },
  { label: "% Operativo",           value: "94.7",   unit: "%",          trend: "up"   as const, change: "+1.1%",  color: T,  Icon: CheckCircle },
  { label: "Tiempo Res. Promedio",  value: "4.2",    unit: "días",       trend: "down" as const, change: "−0.8d",  color: Y,  Icon: Clock },
  { label: "Reportes Ciudadanos",   value: "1,284",  unit: "este mes",   trend: "up"   as const, change: "+18%",   color: B,  Icon: Users },
  { label: "Órdenes Activas",       value: "347",    unit: "",           trend: "down" as const, change: "−12",    color: R,  Icon: Wrench },
  { label: "Presupuesto Ejecutado", value: "68.4",   unit: "%",          trend: "up"   as const, change: "+5.2%",  color: T,  Icon: BarChart2 },
];

const WEEKLY = [
  { day: "Lun", reportes: 42, resueltos: 38 },
  { day: "Mar", reportes: 67, resueltos: 54 },
  { day: "Mié", reportes: 51, resueltos: 49 },
  { day: "Jue", reportes: 89, resueltos: 71 },
  { day: "Vie", reportes: 73, resueltos: 68 },
  { day: "Sáb", reportes: 34, resueltos: 33 },
  { day: "Dom", reportes: 28, resueltos: 27 },
];

const MONTHLY = [
  { month: "Ene", baches: 120, luminarias: 89,  semaforos: 34 },
  { month: "Feb", baches: 98,  luminarias: 102, semaforos: 41 },
  { month: "Mar", baches: 145, luminarias: 76,  semaforos: 28 },
  { month: "Abr", baches: 167, luminarias: 91,  semaforos: 52 },
  { month: "May", baches: 134, luminarias: 108, semaforos: 39 },
  { month: "Jun", baches: 189, luminarias: 124, semaforos: 47 },
  { month: "Jul", baches: 156, luminarias: 97,  semaforos: 43 },
];

const BADGES = [
  { id: 1, name: "Primer Reporte",      emoji: "⭐", unlocked: true,  desc: "Primer reporte ciudadano" },
  { id: 2, name: "Vecino Activo",       emoji: "🏘️", unlocked: true,  desc: "5 reportes en un mes" },
  { id: 3, name: "Guardián del Barrio", emoji: "🛡️", unlocked: true,  desc: "10 reportes verificados" },
  { id: 4, name: "Voz Comunitaria",     emoji: "📢", unlocked: false, desc: "20 votos recibidos" },
  { id: 5, name: "Inspector Pro",       emoji: "🔍", unlocked: false, desc: "50 reportes enviados" },
  { id: 6, name: "Héroe Urbano",        emoji: "🏆", unlocked: false, desc: "100 reportes resueltos" },
];

const LEADERBOARD = [
  { rank: 1, neighborhood: "Piantini",    points: 8420, change: "+120", you: false },
  { rank: 2, neighborhood: "Gazcue",      points: 7890, change: "+85",  you: false },
  { rank: 3, neighborhood: "Naco",        points: 7340, change: "+210", you: true  },
  { rank: 4, neighborhood: "Ens. Ozama",  points: 6780, change: "−40",  you: false },
  { rank: 5, neighborhood: "Villa Mella", points: 6210, change: "+300", you: false },
];

const INVENTORY = [
  { id: "MAT-001", name: "Mezcla asfáltica",      unit: "m³",    stock: 12.5, min: 20  },
  { id: "MAT-002", name: "Luminaria LED 150W",    unit: "unid.", stock: 34,   min: 20  },
  { id: "MAT-003", name: "Señal vial reflectiva", unit: "unid.", stock: 8,    min: 15  },
  { id: "MAT-004", name: "Concreto hidráulico",   unit: "m³",    stock: 28,   min: 10  },
  { id: "MAT-005", name: "Cable conductor AWG-4", unit: "m",     stock: 450,  min: 200 },
];

const MODERATION = [
  { id: "R-4531", type: "pothole",     reporter: "Carlos Mejía",   address: "Av. Luperón #78, La Victoria",              votes: 156, escalated: true,  created: "hace 1h",  priority: "critical" as PriorityKey },
  { id: "R-4529", type: "streetlight", reporter: "Isabel Reyes",   address: "Calle Padre Billini #45, C. Colonial",       votes: 89,  escalated: false, created: "hace 2h",  priority: "high"     as PriorityKey },
  { id: "R-4527", type: "traffic",     reporter: "Juan Pérez",     address: "Av. Rómulo Betancourt esq. Charles de Gaulle", votes: 234, escalated: true,  created: "hace 3h",  priority: "critical" as PriorityKey },
  { id: "R-4525", type: "traffic",     reporter: "María González", address: "Av. JFK esq. Autopista Las Américas",        votes: 67,  escalated: false, created: "hace 4h",  priority: "medium"   as PriorityKey },
];

const CREWS = [
  { id: "N1", name: "Equipo Norte 1", members: 4, status: "active",  orders: 3, district: "Naco / Piantini",         dist: "2.4 km" },
  { id: "N2", name: "Equipo Norte 2", members: 3, status: "transit", orders: 2, district: "Gazcue / C. Colonial",    dist: "5.1 km" },
  { id: "S1", name: "Equipo Sur 1",   members: 5, status: "active",  orders: 4, district: "Villa Mella / Ozama",     dist: "8.2 km" },
  { id: "E1", name: "Equipo Este 1",  members: 3, status: "idle",    orders: 0, district: "Sin asignación",          dist: "—" },
];

// ─── Shared atoms ──────────────────────────────────────────────────────────
function PriorityBadge({ p }: { p: PriorityKey }) {
  const c = PRIORITY[p];
  return (
    <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded text-xs font-semibold"
      style={{ backgroundColor: c.bg, color: c.fg }}>
      <span className="w-1.5 h-1.5 rounded-full inline-block" style={{ backgroundColor: c.dot }} />
      {c.label}
    </span>
  );
}

function StatusBadge({ s }: { s: StatusKey }) {
  const c = STATUS[s] ?? STATUS.pending;
  return (
    <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium"
      style={{ backgroundColor: c.bg, color: c.fg }}>
      {c.label}
    </span>
  );
}

function AssetStatusBadge({ status }: { status: string }) {
  const m: Record<string, { label: string; bg: string; fg: string }> = {
    operational: { label: "Operativo",     bg: "#F0FDF4", fg: "#166534" },
    maintenance: { label: "Mantenimiento", bg: "#FFFBEB", fg: "#856404" },
    critical:    { label: "Crítico",       bg: "#FEE9E3", fg: R },
    offline:     { label: "Sin señal",     bg: "#F3F4F6", fg: "#374151" },
  };
  const c = m[status] ?? m.offline;
  return (
    <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium"
      style={{ backgroundColor: c.bg, color: c.fg }}>{c.label}</span>
  );
}

// ─── City Map SVG ──────────────────────────────────────────────────────────
const MAP_PINS = [
  { x: 180, y: 118, type: "pothole"     },
  { x: 280, y: 158, type: "streetlight" },
  { x: 148, y: 198, type: "traffic"     },
  { x: 322, y: 218, type: "park"        },
  { x: 220, y: 88,  type: "sidewalk"    },
  { x: 98,  y: 152, type: "pothole"     },
  { x: 378, y: 128, type: "streetlight" },
  { x: 258, y: 248, type: "pothole"     },
  { x: 348, y: 78,  type: "traffic"     },
  { x: 128, y: 278, type: "park"        },
  { x: 418, y: 198, type: "sidewalk"    },
  { x: 198, y: 308, type: "pothole"     },
];
const PIN_COLORS: Record<string, string> = {
  pothole: "#7C3AED", streetlight: Y, traffic: R, park: T, sidewalk: G500,
};
const HEAT_SPOTS = [
  { x: 172, y: 128, rx: 72, ry: 55 },
  { x: 278, y: 152, rx: 62, ry: 48 },
  { x: 144, y: 196, rx: 80, ry: 60 },
  { x: 262, y: 248, rx: 65, ry: 50 },
];

function CityMapSVG({ activeLayers, showHeatmap, svgH = 380 }: {
  activeLayers: Set<string>;
  showHeatmap: boolean;
  svgH?: number;
}) {
  return (
    <svg viewBox="0 0 500 380" style={{ width: "100%", height: svgH, display: "block" }}>
      <defs>
        {HEAT_SPOTS.map((_, i) => (
          <radialGradient key={i} id={`hs${i}`} cx="50%" cy="50%" r="50%">
            <stop offset="0%"   stopColor={R}   stopOpacity="0.55" />
            <stop offset="55%"  stopColor={Y}   stopOpacity="0.28" />
            <stop offset="100%" stopColor={Y}   stopOpacity="0" />
          </radialGradient>
        ))}
      </defs>

      {/* Ground */}
      <rect width="500" height="380" fill="#E8E0D0" />

      {/* City blocks */}
      {([
        [20,20,82,78],[118,20,82,58],[218,20,102,48],[338,20,82,58],[438,20,56,68],
        [20,98,92,60],[128,88,70,68],[218,78,92,68],[328,88,82,58],[418,98,76,54],
        [20,178,72,78],[108,168,92,78],[218,158,82,88],[318,168,92,74],[418,163,76,78],
        [20,278,102,78],[138,268,80,60],[238,258,92,78],[348,258,82,70],[438,248,56,78],
      ] as number[][]).map(([x,y,w,h],i) => (
        <rect key={i} x={x} y={y} width={w} height={h} fill="#D4CABD" rx="2" />
      ))}

      {/* Green parks */}
      <rect x="278" y="198" width="62" height="52" fill="#9DC88D" rx="5" />
      <rect x="48"  y="278" width="52" height="42" fill="#9DC88D" rx="5" />

      {/* Caribbean Sea */}
      <rect x="0" y="322" width="500" height="58" fill="#A8CEDE" />
      <text x="20" y="352" fontSize="10" fill="#4A7FA5" fontFamily="Inter,sans-serif" opacity="0.85">Mar Caribe</text>

      {/* Ozama River */}
      <rect x="462" y="0" width="38" height="322" fill="#A8CEDE" />
      <text x="464" y="55" fontSize="9" fill="#4A7FA5" fontFamily="Inter,sans-serif" opacity="0.85"
        transform="rotate(90,464,55)">Río Ozama</text>

      {/* Main avenues */}
      {[
        { x1:0,y1:168,x2:462,y2:168,w:9 },
        { x1:0,y1:258,x2:462,y2:258,w:7 },
        { x1:0,y1:74, x2:462,y2:74, w:5 },
        { x1:208,y1:0,x2:208,y2:322,w:9 },
        { x1:108,y1:0,x2:108,y2:322,w:5 },
        { x1:318,y1:0,x2:318,y2:322,w:7 },
        { x1:418,y1:0,x2:418,y2:322,w:5 },
      ].map((av,i) => (
        <line key={i} x1={av.x1} y1={av.y1} x2={av.x2} y2={av.y2}
          stroke="#F5F0E8" strokeWidth={av.w} />
      ))}

      {/* Secondary streets */}
      {[20,58,138,178,248,290,368,398].map(yy => (
        <line key={`h${yy}`} x1="0" y1={yy} x2="462" y2={yy} stroke="#EDE7D9" strokeWidth="3" />
      ))}
      {[48,158,268,368].map(xx => (
        <line key={`v${xx}`} x1={xx} y1="0" x2={xx} y2="322" stroke="#EDE7D9" strokeWidth="3" />
      ))}

      {/* Heatmap overlay */}
      {showHeatmap && HEAT_SPOTS.map((hs,i) => (
        <ellipse key={i} cx={hs.x} cy={hs.y} rx={hs.rx} ry={hs.ry}
          fill={`url(#hs${i})`} />
      ))}

      {/* Pins */}
      {MAP_PINS.filter(p => activeLayers.has(p.type)).map((pin, i) => (
        <g key={i} transform={`translate(${pin.x},${pin.y})`} style={{ cursor: "pointer" }}>
          <circle r="11" fill={PIN_COLORS[pin.type]} stroke="white" strokeWidth="2.5" />
          <circle r="3.5" fill="white" cy="1" />
        </g>
      ))}

      {/* Compass */}
      <text x="434" y="30" fontSize="13" fill="#4A7FA5" fontFamily="Inter,sans-serif" fontWeight="700">N</text>
      <line x1="440" y1="17" x2="440" y2="33" stroke="#4A7FA5" strokeWidth="1.5" />
    </svg>
  );
}

// ─── Before / After Slider ─────────────────────────────────────────────────
function BeforeAfterSlider() {
  const [pos, setPos] = useState(42);
  const boxRef = useRef<HTMLDivElement>(null);

  const updatePos = useCallback((clientX: number) => {
    if (!boxRef.current) return;
    const rect = boxRef.current.getBoundingClientRect();
    setPos(Math.max(2, Math.min(98, ((clientX - rect.left) / rect.width) * 100)));
  }, []);

  const onMouseDown = (e: React.MouseEvent) => {
    const onMove = (ev: MouseEvent) => updatePos(ev.clientX);
    const onUp = () => {
      document.removeEventListener("mousemove", onMove);
      document.removeEventListener("mouseup", onUp);
    };
    document.addEventListener("mousemove", onMove);
    document.addEventListener("mouseup", onUp);
    updatePos(e.clientX);
  };

  return (
    <div ref={boxRef} className="relative overflow-hidden rounded-xl select-none"
      style={{ height: 200, cursor: "ew-resize" }}
      onMouseDown={onMouseDown}
      onTouchMove={e => updatePos(e.touches[0].clientX)}
      onTouchStart={e => updatePos(e.touches[0].clientX)}>

      {/* Before */}
      <div className="absolute inset-0 bg-gray-700">
        <img src="https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=600&h=400&fit=crop&auto=format"
          alt="Antes de la intervención — calle deteriorada" className="w-full h-full object-cover" />
        <span className="absolute top-3 left-3 text-xs font-bold text-white bg-black/60 px-2 py-1 rounded">ANTES</span>
      </div>

      {/* After */}
      <div className="absolute inset-0 overflow-hidden" style={{ clipPath: `inset(0 ${100 - pos}% 0 0)` }}>
        <img src="https://images.unsplash.com/photo-1486325212027-8081e485255e?w=600&h=400&fit=crop&auto=format"
          alt="Después de la intervención — calle reparada" className="w-full h-full object-cover" />
        <span className="absolute top-3 right-3 text-xs font-bold text-white bg-black/60 px-2 py-1 rounded">DESPUÉS</span>
      </div>

      {/* Handle */}
      <div className="absolute top-0 bottom-0 w-0.5 bg-white shadow-lg pointer-events-none"
        style={{ left: `${pos}%`, transform: "translateX(-50%)" }}>
        <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-9 h-9 bg-white rounded-full shadow-xl flex items-center justify-center gap-0.5">
          <ChevronLeft size={12} className="text-gray-600" />
          <ChevronRight size={12} className="text-gray-600" />
        </div>
      </div>
    </div>
  );
}

// ─── Hurricane Alert Banner ────────────────────────────────────────────────
function HurricaneBanner({ onDismiss }: { onDismiss: () => void }) {
  return (
    <div style={{ backgroundColor: Y, borderBottom: `3px solid ${R}` }}>
      <div className="px-4 py-3">
        <div className="flex items-start gap-3">
          <Wind size={20} style={{ color: R, flexShrink: 0, marginTop: 1 }} />
          <div className="flex-1 min-w-0">
            <p className="font-bold text-sm" style={{ color: "#1A0A00" }}>
              ALERTA METEOROLÓGICA — Tormenta Tropical "Beryl"
            </p>
            <p className="text-xs mt-0.5" style={{ color: "#3D1F00" }}>
              Vientos máximos: 85 km/h · Vigente hasta 9 jul, 20:00 · Evite zonas bajas e inundables
            </p>
            <div className="flex items-center gap-2 mt-2">
              <button className="flex items-center gap-1.5 text-xs font-semibold px-3 py-2 rounded min-h-[36px]"
                style={{ backgroundColor: R, color: "white" }}>
                <Phone size={12} /> 911 Emergencias
              </button>
              <button className="text-xs font-medium underline" style={{ color: "#3D1F00" }}>
                Ver mapa de riesgo
              </button>
            </div>
          </div>
          <button onClick={onDismiss} className="p-1.5 rounded" aria-label="Cerrar alerta"
            style={{ color: "#3D1F00" }}>
            <X size={16} />
          </button>
        </div>
      </div>
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// CITIZEN PORTAL
// ═══════════════════════════════════════════════════════════════════════════

function CitizenOnboarding({ onFinish }: { onFinish: () => void }) {
  const [step, setStep] = useState(0);
  const features = [
    { emoji: "🗺️", title: "Mapa Interactivo",   desc: "Visualiza el estado de la infraestructura urbana de Santo Domingo en tiempo real." },
    { emoji: "📸", title: "Reporta Problemas",   desc: "Fotografía baches, luminarias y semáforos dañados con ubicación GPS automática." },
    { emoji: "📊", title: "Sigue el Progreso",   desc: "Recibe actualizaciones en tiempo real sobre el estado de tus reportes ciudadanos." },
    { emoji: "🏆", title: "Gana Puntos",          desc: "Acumula puntos por reportes verificados y compite con tu vecindario." },
  ];

  if (step === 0) return (
    <div className="flex flex-col h-full">
      <div className="flex-1 flex flex-col items-center justify-center px-6 pt-10 pb-6 text-center"
        style={{ background: `linear-gradient(160deg, ${B} 0%, #003480 100%)` }}>
        <div className="w-20 h-20 rounded-2xl flex items-center justify-center mb-5 text-4xl shadow-lg"
          style={{ backgroundColor: "rgba(255,255,255,0.15)" }}>🏙️</div>
        <h1 className="text-3xl font-extrabold text-white mb-2">UrbanSync</h1>
        <p className="text-sm text-white/75 max-w-xs leading-relaxed">
          La plataforma ciudadana para una Santo Domingo más inteligente y conectada.
        </p>
        <div className="mt-8 grid grid-cols-2 gap-2.5 w-full max-w-sm">
          {features.map((f, i) => (
            <div key={i} className="rounded-xl p-3 text-left"
              style={{ backgroundColor: "rgba(255,255,255,0.12)" }}>
              <div className="text-2xl mb-1">{f.emoji}</div>
              <p className="text-xs font-semibold text-white leading-tight">{f.title}</p>
            </div>
          ))}
        </div>
      </div>
      <div className="p-5 bg-white">
        <button onClick={() => setStep(1)}
          className="w-full py-4 rounded-xl text-base font-bold text-white flex items-center justify-center gap-2 min-h-[52px]"
          style={{ backgroundColor: B }}>
          Comenzar <ChevronRight size={18} />
        </button>
        <button onClick={onFinish} className="w-full mt-2.5 py-2.5 text-sm font-medium" style={{ color: B }}>
          Ya tengo cuenta — Iniciar sesión
        </button>
      </div>
    </div>
  );

  const f = features[step - 1];
  return (
    <div className="flex flex-col h-full bg-white">
      <div className="flex gap-1.5 px-5 pt-6">
        {features.map((_, i) => (
          <div key={i} className="h-1 flex-1 rounded-full transition-all"
            style={{ backgroundColor: i < step ? B : G200 }} />
        ))}
      </div>
      <div className="flex-1 flex flex-col items-center justify-center px-6 text-center">
        <div className="text-7xl mb-5">{f?.emoji}</div>
        <h2 className="text-2xl font-bold mb-3" style={{ color: G900 }}>{f?.title}</h2>
        <p className="text-sm leading-relaxed" style={{ color: G500 }}>{f?.desc}</p>
      </div>
      <div className="p-5">
        <button onClick={() => step < features.length ? setStep(step + 1) : onFinish()}
          className="w-full py-4 rounded-xl font-bold text-white text-base min-h-[52px]"
          style={{ backgroundColor: B }}>
          {step < features.length ? "Siguiente" : "Explorar el mapa"}
        </button>
        <button onClick={() => setStep(s => Math.max(0, s - 1))}
          className="w-full mt-2 py-2.5 text-sm" style={{ color: G500 }}>Atrás</button>
      </div>
    </div>
  );
}

function CitizenMap({ onReport }: { onReport: () => void }) {
  const [activeLayers, setActiveLayers] = useState(new Set(ISSUE_TYPES.map(t => t.id)));
  const [showHeatmap, setShowHeatmap] = useState(false);
  const toggle = (id: string) =>
    setActiveLayers(prev => { const n = new Set(prev); n.has(id) ? n.delete(id) : n.add(id); return n; });

  return (
    <div className="flex flex-col h-full">
      <div className="relative flex-1 overflow-hidden" style={{ backgroundColor: "#E8E0D0" }}>
        <div className="absolute inset-0">
          <CityMapSVG activeLayers={activeLayers} showHeatmap={showHeatmap} svgH={600} />
        </div>

        {/* Controls */}
        <div className="absolute top-3 left-3 right-3 z-10">
          <div className="bg-white rounded-2xl shadow-lg overflow-hidden">
            <div className="flex items-center gap-2 px-3 py-2.5 border-b border-gray-100">
              <Search size={15} className="text-gray-400" />
              <span className="text-sm text-gray-400">Buscar dirección en Santo Domingo...</span>
            </div>
            <div className="flex items-center gap-1.5 px-3 py-2 overflow-x-auto">
              {ISSUE_TYPES.map(t => (
                <button key={t.id} onClick={() => toggle(t.id)}
                  className="flex items-center gap-1 px-2.5 py-1.5 rounded-full text-xs font-semibold whitespace-nowrap min-h-[32px] border transition-all"
                  style={{
                    backgroundColor: activeLayers.has(t.id) ? t.color + "1A" : G50,
                    color: activeLayers.has(t.id) ? t.color : G500,
                    borderColor: activeLayers.has(t.id) ? t.color + "70" : "transparent",
                  }}>
                  <span>{t.emoji}</span>{t.label}
                </button>
              ))}
            </div>
          </div>
        </div>

        {/* Heatmap toggle */}
        <button onClick={() => setShowHeatmap(h => !h)}
          className="absolute z-10 flex items-center gap-1.5 px-3 py-2 rounded-xl shadow-md text-xs font-semibold min-h-[40px] transition-all"
          style={{
            top: 116, right: 16,
            backgroundColor: showHeatmap ? R : "white",
            color: showHeatmap ? "white" : G900,
          }}>
          <Layers size={14} /> Mapa calor
        </button>

        {/* FAB */}
        <button onClick={onReport}
          className="absolute bottom-4 right-4 z-10 flex items-center gap-2 px-4 py-3 rounded-2xl shadow-xl font-bold text-white text-sm min-h-[52px]"
          style={{ backgroundColor: B }}>
          <Plus size={18} /> Nuevo reporte
        </button>
      </div>
    </div>
  );
}

const REPORT_STEPS = ["Categoría", "Foto", "Ubicación", "Descripción", "Confirmar"];

function CitizenReport({ onBack }: { onBack: () => void }) {
  const [step, setStep] = useState(0);
  const [type, setType] = useState<string | null>(null);
  const [desc, setDesc] = useState("");
  const [priority, setPriority] = useState("");

  return (
    <div className="flex flex-col h-full bg-white">
      {/* Header */}
      <div className="flex items-center gap-3 px-4 py-3 border-b border-gray-100">
        <button onClick={step === 0 ? onBack : () => setStep(s => s - 1)}
          className="w-11 h-11 flex items-center justify-center rounded-full"
          style={{ backgroundColor: "#F0F1F4" }} aria-label="Atrás">
          <ChevronLeft size={20} />
        </button>
        <div className="flex-1">
          <p className="text-xs font-medium" style={{ color: G500 }}>Nuevo reporte · Paso {step + 1}/{REPORT_STEPS.length}</p>
          <h2 className="font-bold text-base" style={{ color: G900 }}>{REPORT_STEPS[step]}</h2>
        </div>
      </div>
      <div className="h-1 bg-gray-100">
        <div className="h-full rounded-r-full transition-all" style={{ width: `${((step + 1) / REPORT_STEPS.length) * 100}%`, backgroundColor: B }} />
      </div>

      <div className="flex-1 overflow-y-auto">
        {step === 0 && (
          <div className="p-4 space-y-2">
            <p className="text-sm text-gray-500 mb-3">¿Qué tipo de problema deseas reportar?</p>
            {ISSUE_TYPES.map(t => (
              <button key={t.id} onClick={() => setType(t.id)}
                className="w-full flex items-center gap-4 p-4 rounded-xl border-2 transition-all min-h-[64px] text-left"
                style={{
                  borderColor: type === t.id ? t.color : G200,
                  backgroundColor: type === t.id ? t.color + "0D" : "white",
                }}>
                <span className="text-2xl">{t.emoji}</span>
                <div className="flex-1">
                  <p className="font-semibold text-sm" style={{ color: G900 }}>{t.label}</p>
                  <p className="text-xs" style={{ color: G500 }}>Toca para seleccionar</p>
                </div>
                {type === t.id && (
                  <div className="w-6 h-6 rounded-full flex items-center justify-center" style={{ backgroundColor: t.color }}>
                    <Check size={13} className="text-white" />
                  </div>
                )}
              </button>
            ))}
          </div>
        )}

        {step === 1 && (
          <div className="p-4">
            <p className="text-sm text-gray-500 mb-3">Toma una foto del problema para agilizar la verificación.</p>
            <div className="aspect-video bg-gray-900 rounded-2xl flex flex-col items-center justify-center mb-4 relative overflow-hidden">
              <div className="absolute inset-0 opacity-20" style={{ backgroundImage: "linear-gradient(rgba(255,255,255,0.3) 1px,transparent 1px),linear-gradient(90deg,rgba(255,255,255,0.3) 1px,transparent 1px)", backgroundSize: "40px 40px" }} />
              <Camera size={36} className="text-white/60 mb-3" />
              <p className="text-white/70 text-sm">Vista previa de la cámara</p>
            </div>
            <button className="w-full py-3.5 border-2 border-dashed rounded-xl text-sm font-semibold flex items-center justify-center gap-2 min-h-[52px]"
              style={{ borderColor: B, color: B }}>
              <Camera size={16} /> Abrir cámara
            </button>
            <button className="w-full mt-2 py-3 text-sm" style={{ color: G500 }}>
              Seleccionar de galería
            </button>
          </div>
        )}

        {step === 2 && (
          <div className="p-4">
            <p className="text-sm text-gray-500 mb-3">Tu ubicación GPS fue detectada automáticamente.</p>
            <div className="rounded-2xl overflow-hidden border mb-4" style={{ borderColor: G200, height: 180 }}>
              <CityMapSVG activeLayers={new Set(["pothole"])} showHeatmap={false} svgH={180} />
            </div>
            <div className="flex items-start gap-3 p-3.5 rounded-xl mb-3" style={{ backgroundColor: T + "18" }}>
              <Navigation size={18} style={{ color: T, flexShrink: 0, marginTop: 1 }} />
              <div>
                <p className="font-semibold text-sm" style={{ color: G900 }}>Av. Winston Churchill #342</p>
                <p className="text-xs" style={{ color: G500 }}>Naco, Santo Domingo · Precisión: ±8 m</p>
              </div>
            </div>
            <button className="w-full py-3 border rounded-xl text-sm font-medium min-h-[48px]"
              style={{ borderColor: G200, color: G900 }}>Ajustar ubicación manualmente</button>
          </div>
        )}

        {step === 3 && (
          <div className="p-4 space-y-4">
            <div>
              <label className="block text-sm font-semibold mb-2" style={{ color: G900 }}>Descripción del problema *</label>
              <textarea value={desc} onChange={e => setDesc(e.target.value)}
                rows={5} placeholder="Describe el problema con el mayor detalle posible. ¿Desde cuándo existe? ¿Representa un riesgo inmediato?"
                className="w-full p-3 rounded-xl border text-sm resize-none focus:outline-none focus:ring-2"
                style={{ borderColor: G200 }} />
              <p className="text-xs mt-1 text-right" style={{ color: G500 }}>{desc.length}/300</p>
            </div>
            <div>
              <label className="block text-sm font-semibold mb-2" style={{ color: G900 }}>Prioridad sugerida</label>
              <div className="flex gap-2">
                {(["low","medium","high","critical"] as PriorityKey[]).map(p => (
                  <button key={p} onClick={() => setPriority(p)}
                    className="flex-1 py-2.5 rounded-lg text-xs font-semibold border min-h-[44px] transition-all"
                    style={{
                      borderColor: priority === p ? PRIORITY[p].dot : G200,
                      backgroundColor: priority === p ? PRIORITY[p].bg : G50,
                      color: priority === p ? PRIORITY[p].fg : G500,
                    }}>
                    {PRIORITY[p].label}
                  </button>
                ))}
              </div>
            </div>
          </div>
        )}

        {step === 4 && (
          <div className="p-4">
            <div className="rounded-2xl border overflow-hidden mb-4" style={{ borderColor: G200 }}>
              <div className="p-4 border-b" style={{ borderColor: "#F0F1F4" }}>
                <div className="flex items-center gap-2 mb-1">
                  <span className="text-xl">{ISSUE_TYPES.find(t => t.id === type)?.emoji ?? "📍"}</span>
                  <span className="font-bold" style={{ color: G900 }}>
                    {ISSUE_TYPES.find(t => t.id === type)?.label ?? "Problema"}
                  </span>
                  {priority && <PriorityBadge p={priority as PriorityKey} />}
                </div>
                <p className="text-xs" style={{ color: G500 }}>Av. Winston Churchill #342, Naco</p>
              </div>
              <div className="p-4">
                <p className="text-sm" style={{ color: G900 }}>{desc || "Sin descripción adicional."}</p>
              </div>
            </div>
            <div className="flex items-start gap-2 p-3.5 rounded-xl" style={{ backgroundColor: "#EFF6FF" }}>
              <Info size={15} style={{ color: B, flexShrink: 0, marginTop: 1 }} />
              <p className="text-xs" style={{ color: "#1A3A6B" }}>
                Al enviar aceptas que los datos sean procesados por el Ayuntamiento del Distrito Nacional según la Ley 172-13.
              </p>
            </div>
          </div>
        )}
      </div>

      <div className="p-4 border-t border-gray-100">
        <button
          onClick={() => step < REPORT_STEPS.length - 1 ? setStep(s => s + 1) : onBack()}
          disabled={step === 0 && !type}
          className="w-full py-4 rounded-xl font-bold text-white text-base min-h-[52px] transition-opacity"
          style={{ backgroundColor: B, opacity: step === 0 && !type ? 0.4 : 1 }}>
          {step < REPORT_STEPS.length - 1 ? "Continuar" : "✓ Enviar reporte"}
        </button>
      </div>
    </div>
  );
}

const TIMELINE_STEPS = [
  { label: "Recibido",    desc: "Reporte registrado en el sistema",          time: "6 jul, 10:23" },
  { label: "Verificado",  desc: "Inspector confirmó el problema en campo",   time: "6 jul, 14:45" },
  { label: "Asignado",    desc: "Cuadrilla Equipo Norte 3 asignada",         time: "7 jul, 09:00" },
  { label: "En Progreso", desc: "Cuadrilla trabajando en el sitio",          time: "9 jul, 08:30" },
  { label: "Resuelto",    desc: "Intervención completada y verificada",       time: null },
];

function CitizenTimeline() {
  const current = 3;
  return (
    <div className="flex flex-col h-full bg-white overflow-y-auto">
      <div className="px-4 py-4 border-b border-gray-100">
        <div className="flex items-start justify-between gap-2">
          <div>
            <h2 className="font-bold text-lg" style={{ color: G900 }}>Reporte R-4521</h2>
            <p className="text-sm" style={{ color: G500 }}>Bache profundo · Av. Winston Churchill #342</p>
          </div>
          <PriorityBadge p="high" />
        </div>
      </div>

      <div className="p-4">
        <h3 className="font-bold text-sm mb-4" style={{ color: G900 }}>Estado del reporte</h3>
        <div className="relative pl-10">
          <div className="absolute left-3.5 top-4 bottom-4 w-0.5" style={{ backgroundColor: G200 }} />
          <div className="absolute left-3.5 top-4 w-0.5 transition-all"
            style={{ backgroundColor: B, height: `${(current / (TIMELINE_STEPS.length - 1)) * 80}%` }} />
          <div className="space-y-0">
            {TIMELINE_STEPS.map((s, i) => {
              const done = i <= current;
              const cur = i === current;
              return (
                <div key={i} className="relative pb-7 last:pb-0">
                  <div className="absolute -left-[26px] w-8 h-8 rounded-full border-2 flex items-center justify-center z-10"
                    style={{ backgroundColor: done ? (cur ? B : T) : "white", borderColor: done ? (cur ? B : T) : G200 }}>
                    {done && !cur && <Check size={13} className="text-white" />}
                    {cur && <div className="w-3 h-3 rounded-full bg-white" />}
                  </div>
                  <p className="font-semibold text-sm" style={{ color: done ? G900 : "#9CA3AF" }}>{s.label}</p>
                  <p className="text-xs mt-0.5" style={{ color: done ? G500 : "#D1D5DB" }}>{s.desc}</p>
                  {s.time && <p className="text-xs mt-0.5 font-medium" style={{ color: done ? B : "#D1D5DB" }}>{s.time}</p>}
                  {!s.time && done && <p className="text-xs mt-0.5" style={{ color: "#9CA3AF" }}>Pendiente</p>}
                </div>
              );
            })}
          </div>
        </div>
      </div>

      <div className="px-4 pb-4">
        <h3 className="font-bold text-sm mb-2" style={{ color: G900 }}>Comparativa de intervención</h3>
        <p className="text-xs mb-3" style={{ color: G500 }}>Arrastra el divisor para comparar el antes y después</p>
        <BeforeAfterSlider />
      </div>

      <div className="px-4 pb-6">
        <h3 className="font-bold text-sm mb-3" style={{ color: G900 }}>Actualizaciones</h3>
        {[
          { msg: "La cuadrilla Equipo Norte 3 llegó al sitio y comenzó el bacheo.", time: "9 jul, 08:45", Icon: Wrench },
          { msg: "Tu reporte fue verificado por el inspector Juan M. del distrito Naco.", time: "6 jul, 14:45", Icon: CheckCircle },
          { msg: "Reporte recibido. Número de seguimiento: R-4521.", time: "6 jul, 10:23", Icon: Bell },
        ].map((u, i) => (
          <div key={i} className="flex items-start gap-3 mb-3">
            <div className="w-7 h-7 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5"
              style={{ backgroundColor: "#EFF6FF" }}>
              <u.Icon size={13} style={{ color: B }} />
            </div>
            <div>
              <p className="text-xs" style={{ color: G900 }}>{u.msg}</p>
              <p className="text-xs mt-0.5" style={{ color: G500 }}>{u.time}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function CitizenProfile() {
  return (
    <div className="flex flex-col h-full bg-white overflow-y-auto">
      <div className="px-4 pt-6 pb-5" style={{ background: `linear-gradient(160deg, ${B} 0%, #003480 100%)` }}>
        <div className="flex items-center gap-4 mb-5">
          <div className="w-16 h-16 rounded-2xl flex items-center justify-center text-3xl"
            style={{ backgroundColor: "rgba(255,255,255,0.15)" }}>👷</div>
          <div>
            <h2 className="text-white font-bold text-xl">María González</h2>
            <p className="text-white/70 text-sm">Naco · Santo Domingo</p>
            <span className="inline-flex items-center mt-1 text-xs font-bold px-2.5 py-1 rounded-full"
              style={{ backgroundColor: Y, color: "#1A0A00" }}>🥉 Ciudadano Bronce</span>
          </div>
        </div>
        <div className="rounded-2xl p-4" style={{ backgroundColor: "rgba(255,255,255,0.12)" }}>
          <div className="flex items-end justify-between mb-2">
            <div>
              <p className="text-white/60 text-xs font-medium">Puntos totales</p>
              <p className="text-4xl font-extrabold text-white">2,840</p>
            </div>
            <div className="text-right">
              <p className="text-white/60 text-xs">Próximo nivel</p>
              <p className="text-white font-semibold text-sm">160 pts · Plata</p>
            </div>
          </div>
          <div className="h-2.5 rounded-full" style={{ backgroundColor: "rgba(255,255,255,0.2)" }}>
            <div className="h-full rounded-full" style={{ width: "94.7%", backgroundColor: Y }} />
          </div>
          <div className="flex justify-between mt-1.5">
            <p className="text-white/50 text-xs">0 pts</p>
            <p className="text-white/50 text-xs">Plata: 3,000</p>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-3 divide-x border-b" style={{ borderColor: G200, divideColor: G200 }}>
        {[{ label: "Reportes", value: "23" }, { label: "Verificados", value: "19" }, { label: "Resueltos", value: "14" }].map((s, i) => (
          <div key={i} className="py-4 text-center">
            <p className="text-2xl font-bold" style={{ color: B }}>{s.value}</p>
            <p className="text-xs mt-0.5" style={{ color: G500 }}>{s.label}</p>
          </div>
        ))}
      </div>

      <div className="p-4">
        <h3 className="font-bold text-base mb-3" style={{ color: G900 }}>Insignias</h3>
        <div className="grid grid-cols-3 gap-2.5">
          {BADGES.map(b => (
            <div key={b.id} className="flex flex-col items-center p-3 rounded-xl border"
              style={{
                borderColor: b.unlocked ? B + "40" : G200,
                backgroundColor: b.unlocked ? B + "08" : G50,
              }}>
              <div className="text-3xl mb-1.5" style={{ filter: b.unlocked ? "none" : "grayscale(1)" }}>{b.emoji}</div>
              <p className="text-xs font-semibold text-center leading-tight" style={{ color: b.unlocked ? G900 : "#9CA3AF" }}>
                {b.name}
              </p>
              {!b.unlocked && <p className="text-xs mt-0.5" style={{ color: "#9CA3AF" }}>Bloqueada</p>}
            </div>
          ))}
        </div>
      </div>

      <div className="p-4 pb-6">
        <h3 className="font-bold text-base mb-3" style={{ color: G900 }}>Clasificación por vecindarios</h3>
        <div className="rounded-2xl border overflow-hidden" style={{ borderColor: G200 }}>
          {LEADERBOARD.map((row, i) => (
            <div key={i} className={`flex items-center gap-3 px-4 py-3 ${i < LEADERBOARD.length - 1 ? "border-b" : ""}`}
              style={{ borderColor: G200, backgroundColor: row.you ? B + "0D" : "white" }}>
              <div className="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold flex-shrink-0"
                style={{ backgroundColor: i === 0 ? "#FFD700" : i === 1 ? "#C0C0C0" : i === 2 ? "#CD7F32" : "#F0F1F4", color: i < 3 ? "white" : G500 }}>
                {row.rank}
              </div>
              <p className="flex-1 text-sm font-semibold" style={{ color: G900 }}>
                {row.neighborhood}{row.you && <span className="ml-2 text-xs font-normal" style={{ color: B }}>· Tu barrio</span>}
              </p>
              <p className="text-sm font-bold" style={{ color: G900 }}>{row.points.toLocaleString()}</p>
              <p className="text-xs font-semibold w-10 text-right" style={{ color: row.change.startsWith("+") ? T : R }}>{row.change}</p>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

const CITIZEN_TABS = [
  { id: "onboarding", label: "Inicio",    Icon: Home },
  { id: "map",        label: "Mapa",      Icon: Map },
  { id: "report",     label: "Reportar",  Icon: Plus },
  { id: "timeline",   label: "Mis casos", Icon: FileText },
  { id: "profile",    label: "Perfil",    Icon: User },
] as const;
type CitizenTab = (typeof CITIZEN_TABS)[number]["id"];

function CitizenPortal() {
  const [screen, setScreen] = useState<CitizenTab>("onboarding");
  const [alert, setAlert] = useState(true);
  const [boarded, setBoarded] = useState(false);

  if (!boarded && screen === "onboarding") {
    return <CitizenOnboarding onFinish={() => { setBoarded(true); setScreen("map"); }} />;
  }

  return (
    <div className="flex flex-col h-full">
      {alert && <HurricaneBanner onDismiss={() => setAlert(false)} />}
      <div className="flex-1 overflow-hidden">
        {screen === "map"        && <CitizenMap onReport={() => setScreen("report")} />}
        {screen === "report"     && <CitizenReport onBack={() => setScreen("map")} />}
        {screen === "timeline"   && <CitizenTimeline />}
        {screen === "profile"    && <CitizenProfile />}
        {screen === "onboarding" && <CitizenOnboarding onFinish={() => { setBoarded(true); setScreen("map"); }} />}
      </div>
      <nav className="border-t bg-white" style={{ borderColor: G200 }}>
        <div className="flex">
          {CITIZEN_TABS.map(tab => {
            const active = screen === tab.id;
            const isReport = tab.id === "report";
            return (
              <button key={tab.id} onClick={() => setScreen(tab.id as CitizenTab)}
                className="flex-1 flex flex-col items-center justify-center gap-1 min-h-[64px]"
                aria-current={active ? "page" : undefined}>
                {isReport ? (
                  <div className="w-12 h-12 rounded-full flex items-center justify-center -mt-4 shadow-lg"
                    style={{ backgroundColor: B }}>
                    <tab.Icon size={22} className="text-white" />
                  </div>
                ) : (
                  <tab.Icon size={20} style={{ color: active ? B : "#9CA3AF" }} />
                )}
                <span className="text-xs font-medium" style={{ color: active ? B : "#9CA3AF" }}>{tab.label}</span>
              </button>
            );
          })}
        </div>
      </nav>
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// TECHNICIAN APP
// ═══════════════════════════════════════════════════════════════════════════

function TechOrders({ onSelect }: { onSelect: (id: string) => void }) {
  const [search, setSearch] = useState("");
  const [filter, setFilter] = useState("all");

  const filtered = WORK_ORDERS.filter(wo => {
    const q = search.toLowerCase();
    return (wo.title.toLowerCase().includes(q) || wo.address.toLowerCase().includes(q)) &&
      (filter === "all" || wo.priority === filter);
  });

  return (
    <div className="flex flex-col h-full" style={{ backgroundColor: G50 }}>
      <div className="bg-white border-b px-4 py-3" style={{ borderColor: G200 }}>
        <h1 className="font-bold text-xl" style={{ color: G900 }}>Órdenes de Trabajo</h1>
        <p className="text-xs" style={{ color: G500 }}>Carlos Méndez · Zona Norte · Hoy, 9 jul</p>
      </div>

      <div className="px-4 py-3 bg-white border-b space-y-2" style={{ borderColor: "#F0F1F4" }}>
        <div className="flex items-center gap-2 px-3 py-2 rounded-xl" style={{ backgroundColor: "#F0F1F4" }}>
          <Search size={15} className="text-gray-400" />
          <input value={search} onChange={e => setSearch(e.target.value)}
            placeholder="Buscar orden..." className="flex-1 bg-transparent text-sm outline-none" />
        </div>
        <div className="flex gap-1.5">
          {["all", "critical", "high", "medium"].map(f => (
            <button key={f} onClick={() => setFilter(f)}
              className="px-3 py-1.5 rounded-full text-xs font-semibold min-h-[32px] transition-all"
              style={{ backgroundColor: filter === f ? B : "#F0F1F4", color: filter === f ? "white" : G500 }}>
              {f === "all" ? "Todas" : PRIORITY[f as PriorityKey].label}
            </button>
          ))}
        </div>
      </div>

      <div className="mx-4 mt-3 flex items-center gap-2 px-3 py-2 rounded-lg"
        style={{ backgroundColor: Y + "22", border: `1px solid ${Y}60` }}>
        <WifiOff size={14} style={{ color: "#856404" }} />
        <p className="text-xs font-medium flex-1" style={{ color: "#856404" }}>Modo sin conexión · Último sync: hace 12 min</p>
        <RefreshCw size={12} style={{ color: "#856404" }} />
      </div>

      <div className="flex-1 overflow-y-auto px-4 py-2 space-y-2 pb-4">
        {filtered.map(wo => (
          <button key={wo.id} onClick={() => onSelect(wo.id)}
            className="w-full bg-white rounded-2xl shadow-sm text-left p-4"
            style={{ border: `1px solid ${wo.priority === "critical" ? R + "50" : G200}`, borderLeftWidth: 4, borderLeftColor: PRIORITY[wo.priority].dot }}>
            <div className="flex items-center justify-between gap-2 mb-2">
              <div className="flex items-center gap-1.5">
                <PriorityBadge p={wo.priority} />
                <StatusBadge s={wo.status} />
              </div>
              <span className="text-xs font-mono" style={{ color: G500 }}>{wo.id}</span>
            </div>
            <p className="font-semibold text-sm mb-1" style={{ color: G900 }}>{wo.title}</p>
            <p className="text-xs mb-2" style={{ color: G500 }}>{wo.address}</p>
            <div className="flex items-center gap-4 text-xs" style={{ color: G500 }}>
              <span className="flex items-center gap-1"><Clock size={11} />{wo.created}</span>
              <span className="flex items-center gap-1"><MapPin size={11} />{wo.district}</span>
            </div>
          </button>
        ))}
      </div>
    </div>
  );
}

function TechOrderDetail({ orderId, onBack, onIntervene }: { orderId: string; onBack: () => void; onIntervene: () => void }) {
  const wo = WORK_ORDERS.find(w => w.id === orderId) ?? WORK_ORDERS[0];
  return (
    <div className="flex flex-col h-full bg-white">
      <div className="flex items-center gap-3 px-4 py-3 border-b" style={{ borderColor: G200 }}>
        <button onClick={onBack} className="w-11 h-11 flex items-center justify-center rounded-full"
          style={{ backgroundColor: "#F0F1F4" }} aria-label="Atrás"><ChevronLeft size={20} /></button>
        <div className="flex-1">
          <p className="text-xs" style={{ color: G500 }}>{wo.id}</p>
          <h2 className="font-bold text-base" style={{ color: G900 }}>Detalle de Orden</h2>
        </div>
        <PriorityBadge p={wo.priority} />
      </div>

      <div className="flex-1 overflow-y-auto">
        <div style={{ height: 180 }}>
          <CityMapSVG activeLayers={new Set([wo.type])} showHeatmap={false} svgH={180} />
        </div>

        <div className="p-4">
          <h3 className="font-bold text-lg mb-1" style={{ color: G900 }}>{wo.title}</h3>
          <p className="text-sm flex items-center gap-1.5 mb-4" style={{ color: G500 }}>
            <MapPin size={13} />{wo.address}
          </p>

          <div className="grid grid-cols-2 gap-2.5 mb-5">
            {[
              { label: "Estado",   val: <StatusBadge s={wo.status} /> },
              { label: "Técnico",  val: wo.assignee ?? "Sin asignar" },
              { label: "Distrito", val: wo.district },
              { label: "Vence",    val: wo.dueDate },
            ].map(({ label, val }, i) => (
              <div key={i} className="rounded-xl p-3" style={{ backgroundColor: G50 }}>
                <p className="text-xs mb-1" style={{ color: G500 }}>{label}</p>
                {typeof val === "string"
                  ? <p className="text-sm font-semibold" style={{ color: G900 }}>{val}</p>
                  : val}
              </div>
            ))}
          </div>

          <h3 className="font-bold text-sm mb-3" style={{ color: G900 }}>Historial del activo</h3>
          <div className="space-y-3">
            {[
              { event: "Último mantenimiento preventivo",    date: "15 jun 2026", actor: "Equipo Sur 2",      Icon: Wrench },
              { event: "Reporte ciudadano creado",           date: "6 jul 2026",  actor: "María G.",           Icon: AlertCircle },
              { event: "Verificación en campo",              date: "6 jul 2026",  actor: "Inspector Juan M.",  Icon: Eye },
              { event: "Asignación de cuadrilla (Auto)",     date: "7 jul 2026",  actor: "Sistema UrbanSync",  Icon: Truck },
            ].map((h, i) => (
              <div key={i} className="flex items-start gap-3">
                <div className="w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0"
                  style={{ backgroundColor: "#EFF6FF" }}>
                  <h.Icon size={13} style={{ color: B }} />
                </div>
                <div>
                  <p className="text-sm font-medium" style={{ color: G900 }}>{h.event}</p>
                  <p className="text-xs" style={{ color: G500 }}>{h.date} · {h.actor}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="p-4 border-t" style={{ borderColor: G200 }}>
        <button onClick={onIntervene}
          className="w-full py-4 rounded-xl font-bold text-white text-base min-h-[52px] flex items-center justify-center gap-2"
          style={{ backgroundColor: T }}>
          <Wrench size={18} /> Iniciar intervención
        </button>
      </div>
    </div>
  );
}

const INTERV_STEPS = ["Foto Antes", "Intervención", "Materiales", "Foto Después"];

function TechIntervention({ onBack }: { onBack: () => void }) {
  const [step, setStep] = useState(0);
  const [notes, setNotes] = useState("");
  const [qty, setQty] = useState<Record<string, number>>({});

  return (
    <div className="flex flex-col h-full bg-white">
      <div className="flex items-center gap-3 px-4 py-3 border-b" style={{ borderColor: G200 }}>
        <button onClick={step === 0 ? onBack : () => setStep(s => s - 1)}
          className="w-11 h-11 flex items-center justify-center rounded-full"
          style={{ backgroundColor: "#F0F1F4" }} aria-label="Atrás"><ChevronLeft size={20} /></button>
        <div className="flex-1">
          <p className="text-xs" style={{ color: G500 }}>WO-2847 · Paso {step + 1}/{INTERV_STEPS.length}</p>
          <h2 className="font-bold text-base" style={{ color: G900 }}>{INTERV_STEPS[step]}</h2>
        </div>
      </div>
      <div className="flex gap-1 px-4 py-2">
        {INTERV_STEPS.map((_, i) => (
          <div key={i} className="flex-1 h-1.5 rounded-full transition-all"
            style={{ backgroundColor: i <= step ? T : G200 }} />
        ))}
      </div>

      <div className="flex-1 overflow-y-auto p-4">
        {(step === 0 || step === 3) && (
          <>
            <p className="text-sm mb-4" style={{ color: G500 }}>
              {step === 0 ? "Documenta el estado actual antes de iniciar la reparación." : "Fotografía el resultado final para cerrar la orden."}
            </p>
            <div className="aspect-video bg-gray-900 rounded-2xl flex flex-col items-center justify-center mb-4">
              <Camera size={38} className="text-white/40 mb-3" />
              <p className="text-white/60 text-sm">{step === 0 ? "Foto ANTES de la intervención" : "Foto DESPUÉS de la intervención"}</p>
            </div>
            <button className="w-full py-4 rounded-xl border-2 border-dashed flex items-center justify-center gap-2 font-semibold min-h-[52px]"
              style={{ borderColor: T, color: T }}>
              <Camera size={18} /> Capturar foto
            </button>
            <button className="w-full mt-2 py-3 text-sm" style={{ color: G500 }}>Seleccionar de galería</button>
          </>
        )}

        {step === 1 && (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-semibold mb-2" style={{ color: G900 }}>Tipo de intervención</label>
              <div className="space-y-2">
                {["Bacheo superficial", "Bacheo profundo (refuerzo de base)", "Reparación parcial de losa", "Señalización temporal"].map(t => (
                  <button key={t} className="w-full flex items-center gap-3 p-3.5 rounded-xl border text-left min-h-[52px]"
                    style={{ borderColor: G200 }}>
                    <div className="w-5 h-5 rounded border-2 flex items-center justify-center flex-shrink-0"
                      style={{ borderColor: "#CBCCD4" }} />
                    <span className="text-sm" style={{ color: G900 }}>{t}</span>
                  </button>
                ))}
              </div>
            </div>
            <div>
              <label className="block text-sm font-semibold mb-2" style={{ color: G900 }}>Notas técnicas</label>
              <textarea value={notes} onChange={e => setNotes(e.target.value)} rows={4}
                placeholder="Observaciones del técnico responsable..."
                className="w-full p-3 rounded-xl border text-sm resize-none focus:outline-none"
                style={{ borderColor: G200 }} />
            </div>
          </div>
        )}

        {step === 2 && (
          <>
            <p className="text-sm mb-4" style={{ color: G500 }}>Registra los materiales utilizados en esta intervención.</p>
            <div className="space-y-2">
              {INVENTORY.slice(0, 4).map(m => (
                <div key={m.id} className="flex items-center gap-3 p-3.5 rounded-xl" style={{ backgroundColor: G50 }}>
                  <div className="flex-1">
                    <p className="text-sm font-medium" style={{ color: G900 }}>{m.name}</p>
                    <p className="text-xs" style={{ color: G500 }}>Stock: {m.stock} {m.unit}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    <button className="w-9 h-9 rounded-full border flex items-center justify-center font-bold"
                      style={{ borderColor: G200 }}
                      onClick={() => setQty(p => ({ ...p, [m.id]: Math.max(0, (p[m.id] ?? 0) - 1) }))}>−</button>
                    <span className="w-6 text-center text-sm font-bold" style={{ color: G900 }}>{qty[m.id] ?? 0}</span>
                    <button className="w-9 h-9 rounded-full border flex items-center justify-center font-bold"
                      style={{ borderColor: G200 }}
                      onClick={() => setQty(p => ({ ...p, [m.id]: (p[m.id] ?? 0) + 1 }))}>+</button>
                  </div>
                </div>
              ))}
            </div>
          </>
        )}
      </div>

      <div className="p-4 border-t" style={{ borderColor: G200 }}>
        <button onClick={() => step < INTERV_STEPS.length - 1 ? setStep(s => s + 1) : onBack()}
          className="w-full py-4 rounded-xl font-bold text-white text-base min-h-[52px]"
          style={{ backgroundColor: T }}>
          {step < INTERV_STEPS.length - 1 ? "Siguiente" : "✓ Completar intervención"}
        </button>
      </div>
    </div>
  );
}

function TechInventory({ onBack }: { onBack: () => void }) {
  const lowCount = INVENTORY.filter(m => m.stock < m.min).length;
  return (
    <div className="flex flex-col h-full" style={{ backgroundColor: G50 }}>
      <div className="flex items-center gap-3 px-4 py-3 bg-white border-b" style={{ borderColor: G200 }}>
        <button onClick={onBack} className="w-11 h-11 flex items-center justify-center rounded-full"
          style={{ backgroundColor: "#F0F1F4" }} aria-label="Atrás"><ChevronLeft size={20} /></button>
        <h2 className="font-bold text-lg" style={{ color: G900 }}>Inventario y Materiales</h2>
      </div>

      {lowCount > 0 && (
        <div className="mx-4 mt-4 p-3 rounded-xl flex items-center gap-3"
          style={{ backgroundColor: R + "15", border: `1px solid ${R}40` }}>
          <AlertTriangle size={15} style={{ color: R }} />
          <p className="text-sm font-semibold" style={{ color: R }}>
            {lowCount} material{lowCount > 1 ? "es" : ""} por debajo del stock mínimo
          </p>
        </div>
      )}

      <div className="flex-1 overflow-y-auto px-4 py-3 space-y-2 pb-4">
        {INVENTORY.map(m => {
          const pct = Math.min(100, (m.stock / m.min) * 100);
          const low = m.stock < m.min;
          return (
            <div key={m.id} className="bg-white rounded-2xl p-4 shadow-sm border"
              style={{ borderColor: low ? R + "50" : G200 }}>
              <div className="flex items-start justify-between mb-2">
                <div>
                  <p className="font-semibold text-sm" style={{ color: G900 }}>{m.name}</p>
                  <p className="text-xs" style={{ color: G500 }}>{m.id}</p>
                </div>
                <span className="text-xs font-semibold px-2 py-0.5 rounded-full"
                  style={{ backgroundColor: low ? R + "15" : T + "15", color: low ? R : T }}>
                  {low ? "Stock bajo" : "OK"}
                </span>
              </div>
              <div className="flex items-baseline justify-between mb-2">
                <p className="text-xl font-bold" style={{ color: low ? R : G900 }}>
                  {m.stock} <span className="text-sm font-normal" style={{ color: G500 }}>{m.unit}</span>
                </p>
                <p className="text-xs" style={{ color: G500 }}>Mín: {m.min} {m.unit}</p>
              </div>
              <div className="h-2 rounded-full overflow-hidden" style={{ backgroundColor: G200 }}>
                <div className="h-full rounded-full" style={{ width: `${pct}%`, backgroundColor: low ? R : T }} />
              </div>
            </div>
          );
        })}
      </div>

      <div className="p-4 border-t bg-white" style={{ borderColor: G200 }}>
        <button className="w-full py-4 rounded-xl font-bold text-base min-h-[52px] border-2"
          style={{ borderColor: B, color: B }}>
          Solicitar reposición de materiales
        </button>
      </div>
    </div>
  );
}

const TECH_TABS = [
  { id: "orders",    label: "Órdenes",    Icon: List },
  { id: "inventory", label: "Inventario", Icon: Package },
  { id: "map",       label: "Mapa",       Icon: Map },
  { id: "profile",   label: "Perfil",     Icon: User },
] as const;
type TechScreen = (typeof TECH_TABS)[number]["id"] | "detail" | "intervention";

function TechnicianApp() {
  const [screen, setScreen] = useState<TechScreen>("orders");
  const [selectedId, setSelectedId] = useState<string | null>(null);

  return (
    <div className="flex flex-col h-full" style={{ backgroundColor: G50 }}>
      <div className="flex-1 overflow-hidden">
        {screen === "orders"       && <TechOrders onSelect={id => { setSelectedId(id); setScreen("detail"); }} />}
        {screen === "detail"       && selectedId && <TechOrderDetail orderId={selectedId} onBack={() => setScreen("orders")} onIntervene={() => setScreen("intervention")} />}
        {screen === "intervention" && <TechIntervention onBack={() => setScreen("orders")} />}
        {screen === "inventory"    && <TechInventory onBack={() => setScreen("orders")} />}
        {screen === "map"          && (
          <div className="h-full" style={{ backgroundColor: "#E8E0D0" }}>
            <CityMapSVG activeLayers={new Set(ISSUE_TYPES.map(t => t.id))} showHeatmap={false} svgH={700} />
          </div>
        )}
        {screen === "profile" && (
          <div className="h-full bg-white flex flex-col items-center justify-center gap-2 p-6">
            <div className="text-6xl mb-2">👷</div>
            <h2 className="font-bold text-2xl" style={{ color: G900 }}>Carlos Méndez</h2>
            <p style={{ color: G500 }}>Técnico de Infraestructura Vial</p>
            <p className="text-sm" style={{ color: G500 }}>Zona Norte · ADN</p>
            <div className="mt-4 px-4 py-2 rounded-full text-sm font-semibold"
              style={{ backgroundColor: T + "15", color: T }}>● En servicio</div>
          </div>
        )}
      </div>

      {!["detail", "intervention"].includes(screen) && (
        <nav className="border-t bg-white" style={{ borderColor: G200 }}>
          <div className="flex">
            {TECH_TABS.map(tab => {
              const active = screen === tab.id;
              return (
                <button key={tab.id} onClick={() => setScreen(tab.id as TechScreen)}
                  className="flex-1 flex flex-col items-center justify-center gap-1 min-h-[64px]"
                  aria-current={active ? "page" : undefined}>
                  <tab.Icon size={20} style={{ color: active ? T : "#9CA3AF" }} />
                  <span className="text-xs font-medium" style={{ color: active ? T : "#9CA3AF" }}>{tab.label}</span>
                </button>
              );
            })}
          </div>
        </nav>
      )}
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// ADMIN DASHBOARD
// ═══════════════════════════════════════════════════════════════════════════

const ADMIN_NAV = [
  { id: "kpis",       label: "Panel principal",   Icon: BarChart2 },
  { id: "map",        label: "Mapa de la ciudad", Icon: Map },
  { id: "tables",     label: "Activos y órdenes", Icon: List },
  { id: "routes",     label: "Rutas de cuadrillas",Icon: Route },
  { id: "moderation", label: "Moderación",         Icon: AlertCircle },
] as const;
type AdminTab = (typeof ADMIN_NAV)[number]["id"];

function AdminKPIs() {
  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-2xl font-extrabold" style={{ color: G900 }}>Panel Principal</h1>
        <p className="text-sm mt-0.5" style={{ color: G500 }}>Distrito Nacional · Actualizado: 9 jul 2026, 09:47</p>
      </div>

      <div className="grid grid-cols-3 gap-4">
        {KPIS.map((k, i) => (
          <div key={i} className="bg-white rounded-2xl p-5 shadow-sm border" style={{ borderColor: G200 }}>
            <div className="flex items-start justify-between mb-3">
              <div className="w-10 h-10 rounded-xl flex items-center justify-center"
                style={{ backgroundColor: k.color + "18" }}>
                <k.Icon size={20} style={{ color: k.color }} />
              </div>
              <div className="flex items-center gap-1 text-xs font-semibold"
                style={{ color: (k.trend === "down" && k.label.includes("Tiempo")) || k.trend === "up" ? T : R }}>
                {k.trend === "up" ? <TrendingUp size={13} /> : <TrendingDown size={13} />}
                {k.change}
              </div>
            </div>
            <p className="text-3xl font-extrabold mb-0.5" style={{ color: G900 }}>
              {k.value}<span className="text-sm font-normal ml-1" style={{ color: G500 }}>{k.unit}</span>
            </p>
            <p className="text-sm" style={{ color: G500 }}>{k.label}</p>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-5 gap-4">
        <div className="col-span-3 bg-white rounded-2xl p-5 shadow-sm border" style={{ borderColor: G200 }}>
          <h3 className="font-bold text-base mb-4" style={{ color: G900 }}>Reportes vs. Resueltos esta semana</h3>
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={WEEKLY} barCategoryGap="35%">
              <CartesianGrid strokeDasharray="3 3" stroke="#F0F1F4" vertical={false} />
              <XAxis dataKey="day" tick={{ fontSize: 11, fill: G500 }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: G500 }} axisLine={false} tickLine={false} />
              <Tooltip contentStyle={{ borderRadius: 10, border: `1px solid ${G200}`, fontSize: 12 }} />
              <Bar dataKey="reportes" name="Reportes" fill={B} radius={[5, 5, 0, 0]} />
              <Bar dataKey="resueltos" name="Resueltos" fill={T} radius={[5, 5, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
        <div className="col-span-2 bg-white rounded-2xl p-5 shadow-sm border" style={{ borderColor: G200 }}>
          <h3 className="font-bold text-base mb-4" style={{ color: G900 }}>Tendencia mensual 2026</h3>
          <ResponsiveContainer width="100%" height={200}>
            <AreaChart data={MONTHLY}>
              <CartesianGrid strokeDasharray="3 3" stroke="#F0F1F4" vertical={false} />
              <XAxis dataKey="month" tick={{ fontSize: 10, fill: G500 }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 10, fill: G500 }} axisLine={false} tickLine={false} />
              <Tooltip contentStyle={{ borderRadius: 10, border: `1px solid ${G200}`, fontSize: 11 }} />
              <Area type="monotone" dataKey="baches" name="Baches" stroke={B} fill={B + "28"} strokeWidth={2} />
              <Area type="monotone" dataKey="luminarias" name="Luminarias" stroke={T} fill={T + "28"} strokeWidth={2} />
            </AreaChart>
          </ResponsiveContainer>
        </div>
      </div>

      <div className="grid grid-cols-3 gap-4">
        {[
          { label: "Reportes pendientes moderación",  value: "28",   color: Y, note: "3 escalados" },
          { label: "Activos en mantenimiento",         value: "142",  color: B, note: "vs 138 sem. ant." },
          { label: "Satisfacción ciudadana",           value: "87%",  color: T, note: "↑ +3% vs jun" },
        ].map((s, i) => (
          <div key={i} className="bg-white rounded-2xl p-4 shadow-sm border flex items-center gap-4"
            style={{ borderColor: G200 }}>
            <div className="w-12 h-12 rounded-full flex items-center justify-center flex-shrink-0 text-xl font-extrabold"
              style={{ backgroundColor: s.color + "18", color: s.color }}>{s.value.charAt(0)}</div>
            <div>
              <p className="text-2xl font-extrabold" style={{ color: G900 }}>{s.value}</p>
              <p className="text-xs" style={{ color: G500 }}>{s.label}</p>
              <p className="text-xs font-medium mt-0.5" style={{ color: s.color }}>{s.note}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

function AdminMapView() {
  const [activeLayers, setActiveLayers] = useState(new Set(ISSUE_TYPES.map(t => t.id)));
  const [showHeatmap, setShowHeatmap] = useState(false);
  const [clustering, setClustering] = useState(true);
  const toggle = (id: string) =>
    setActiveLayers(prev => { const n = new Set(prev); n.has(id) ? n.delete(id) : n.add(id); return n; });

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-extrabold" style={{ color: G900 }}>Mapa de la Ciudad</h1>
          <p className="text-sm mt-0.5" style={{ color: G500 }}>Santo Domingo · {activeLayers.size} capa{activeLayers.size !== 1 ? "s" : ""} activa{activeLayers.size !== 1 ? "s" : ""}</p>
        </div>
        <div className="flex gap-2">
          <button onClick={() => setClustering(c => !c)}
            className="flex items-center gap-2 px-4 py-2 rounded-lg border text-sm font-semibold min-h-[40px] transition-all"
            style={{ backgroundColor: clustering ? B : "white", color: clustering ? "white" : G900, borderColor: clustering ? B : G200 }}>
            <Users size={14} /> Clustering
          </button>
          <button onClick={() => setShowHeatmap(h => !h)}
            className="flex items-center gap-2 px-4 py-2 rounded-lg border text-sm font-semibold min-h-[40px] transition-all"
            style={{ backgroundColor: showHeatmap ? R : "white", color: showHeatmap ? "white" : G900, borderColor: showHeatmap ? R : G200 }}>
            <Layers size={14} /> Mapa de calor
          </button>
        </div>
      </div>

      <div className="bg-white rounded-2xl shadow-sm border overflow-hidden" style={{ borderColor: G200 }}>
        <div className="px-4 py-3 border-b flex flex-wrap gap-2" style={{ borderColor: "#F0F1F4" }}>
          {ISSUE_TYPES.map(t => (
            <button key={t.id} onClick={() => toggle(t.id)}
              className="flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs font-semibold border min-h-[32px] transition-all"
              style={{
                backgroundColor: activeLayers.has(t.id) ? t.color + "15" : G50,
                color: activeLayers.has(t.id) ? t.color : G500,
                borderColor: activeLayers.has(t.id) ? t.color + "60" : "transparent",
              }}>
              {t.emoji} {t.label}
              {activeLayers.has(t.id) && <Check size={10} />}
            </button>
          ))}
        </div>
        <CityMapSVG activeLayers={activeLayers} showHeatmap={showHeatmap} svgH={460} />
      </div>
    </div>
  );
}

function AdminTables() {
  const [tab, setTab] = useState<"orders" | "assets">("orders");
  const [search, setSearch] = useState("");
  const [sortCol, setSortCol] = useState("id");
  const [sortDir, setSortDir] = useState<"asc" | "desc">("asc");
  const [page, setPage] = useState(1);
  const PER = 5;

  const handleSort = (col: string) => {
    if (sortCol === col) setSortDir(d => d === "asc" ? "desc" : "asc");
    else { setSortCol(col); setSortDir("asc"); }
  };
  const SortIcon = ({ col }: { col: string }) => (
    <span style={{ opacity: 0.5, fontSize: 10 }}>{sortCol === col ? (sortDir === "asc" ? "↑" : "↓") : "↕"}</span>
  );

  const q = search.toLowerCase();
  const orders = WORK_ORDERS.filter(wo => wo.title.toLowerCase().includes(q) || wo.district.toLowerCase().includes(q));
  const assets = ASSETS.filter(a => a.type.toLowerCase().includes(q) || a.location.toLowerCase().includes(q));
  const data = tab === "orders" ? orders : assets;
  const total = data.length;
  const pages = Math.max(1, Math.ceil(total / PER));
  const sliced = tab === "orders" ? orders.slice((page - 1) * PER, page * PER) : assets.slice((page - 1) * PER, page * PER);

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-extrabold" style={{ color: G900 }}>Activos y Órdenes de Trabajo</h1>
        <p className="text-sm mt-0.5" style={{ color: G500 }}>Gestión completa del inventario y seguimiento de intervenciones</p>
      </div>

      <div className="flex gap-1 bg-gray-100 p-1 rounded-xl w-fit">
        {[{ id: "orders", label: "Órdenes de Trabajo" }, { id: "assets", label: "Activos Urbanos" }].map(t => (
          <button key={t.id} onClick={() => { setTab(t.id as any); setPage(1); setSearch(""); }}
            className="px-4 py-2 rounded-lg text-sm font-semibold transition-all min-h-[40px]"
            style={{
              backgroundColor: tab === t.id ? "white" : "transparent",
              color: tab === t.id ? G900 : G500,
              boxShadow: tab === t.id ? "0 1px 3px rgba(0,0,0,0.1)" : "none",
            }}>{t.label}</button>
        ))}
      </div>

      <div className="bg-white rounded-2xl shadow-sm border" style={{ borderColor: G200 }}>
        <div className="flex items-center gap-3 p-4 border-b" style={{ borderColor: "#F0F1F4" }}>
          <div className="flex items-center gap-2 px-3 py-2 rounded-lg flex-1" style={{ backgroundColor: G50 }}>
            <Search size={14} className="text-gray-400" />
            <input value={search} onChange={e => { setSearch(e.target.value); setPage(1); }}
              placeholder={tab === "orders" ? "Buscar órdenes..." : "Buscar activos..."}
              className="bg-transparent text-sm outline-none flex-1" />
          </div>
          <button className="flex items-center gap-2 px-3 py-2 rounded-lg border text-sm min-h-[40px]"
            style={{ borderColor: G200 }}>
            <Filter size={13} /> Filtros
          </button>
          <button className="flex items-center gap-2 px-3 py-2 rounded-lg border text-sm min-h-[40px]"
            style={{ borderColor: G200 }}>
            <Download size={13} /> Exportar
          </button>
        </div>

        <div className="overflow-x-auto">
          {tab === "orders" ? (
            <table className="w-full">
              <thead>
                <tr style={{ backgroundColor: "#F9FAFB" }}>
                  {["ID", "Tipo", "Descripción", "Prioridad", "Estado", "Técnico", "Vence"].map(col => (
                    <th key={col} onClick={() => handleSort(col.toLowerCase())}
                      className="px-4 py-3 text-left text-xs font-bold uppercase tracking-wide cursor-pointer select-none"
                      style={{ color: G500 }}>
                      {col} <SortIcon col={col.toLowerCase()} />
                    </th>
                  ))}
                  <th className="px-4 py-3 text-right text-xs font-bold uppercase" style={{ color: G500 }}>Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y" style={{ borderColor: "#F9FAFB" }}>
                {(sliced as typeof WORK_ORDERS).map(wo => (
                  <tr key={wo.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-4 py-3 text-sm font-mono font-semibold" style={{ color: B }}>{wo.id}</td>
                    <td className="px-4 py-3 text-xl">{ISSUE_TYPES.find(t => t.id === wo.type)?.emoji}</td>
                    <td className="px-4 py-3">
                      <p className="text-sm font-medium" style={{ color: G900 }}>{wo.title}</p>
                      <p className="text-xs mt-0.5" style={{ color: G500 }}>{wo.district}</p>
                    </td>
                    <td className="px-4 py-3"><PriorityBadge p={wo.priority} /></td>
                    <td className="px-4 py-3"><StatusBadge s={wo.status} /></td>
                    <td className="px-4 py-3 text-sm" style={{ color: G500 }}>{wo.assignee ?? <span className="text-gray-300">—</span>}</td>
                    <td className="px-4 py-3 text-sm" style={{ color: G500 }}>{wo.dueDate}</td>
                    <td className="px-4 py-3 text-right">
                      <button className="p-1.5 rounded hover:bg-gray-100" style={{ color: G500 }}><MoreVertical size={15} /></button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : (
            <table className="w-full">
              <thead>
                <tr style={{ backgroundColor: "#F9FAFB" }}>
                  {["ID", "Tipo", "Ubicación", "Estado", "Último Mant.", "Próximo Mant."].map(col => (
                    <th key={col} onClick={() => handleSort(col.toLowerCase())}
                      className="px-4 py-3 text-left text-xs font-bold uppercase tracking-wide cursor-pointer select-none"
                      style={{ color: G500 }}>
                      {col} <SortIcon col={col.toLowerCase()} />
                    </th>
                  ))}
                  <th className="px-4 py-3 text-right text-xs font-bold uppercase" style={{ color: G500 }}>Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y" style={{ borderColor: "#F9FAFB" }}>
                {(sliced as typeof ASSETS).map(a => (
                  <tr key={a.id} className="hover:bg-gray-50 transition-colors">
                    <td className="px-4 py-3 text-sm font-mono font-semibold" style={{ color: B }}>{a.id}</td>
                    <td className="px-4 py-3 text-sm font-medium" style={{ color: G900 }}>{a.type}</td>
                    <td className="px-4 py-3 text-sm" style={{ color: G500 }}>{a.location}</td>
                    <td className="px-4 py-3"><AssetStatusBadge status={a.status} /></td>
                    <td className="px-4 py-3 text-sm" style={{ color: G500 }}>{a.lastMaint}</td>
                    <td className="px-4 py-3 text-sm" style={{ color: G500 }}>{a.nextMaint}</td>
                    <td className="px-4 py-3 text-right">
                      <button className="p-1.5 rounded hover:bg-gray-100" style={{ color: G500 }}><MoreVertical size={15} /></button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        <div className="flex items-center justify-between px-4 py-3 border-t" style={{ borderColor: "#F0F1F4" }}>
          <p className="text-sm" style={{ color: G500 }}>
            {Math.min((page - 1) * PER + 1, total)}–{Math.min(page * PER, total)} de {total} registros
          </p>
          <div className="flex gap-1">
            <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1}
              className="w-8 h-8 flex items-center justify-center rounded border text-sm disabled:opacity-30"
              style={{ borderColor: G200 }}>
              <ChevronLeft size={13} />
            </button>
            {Array.from({ length: pages }, (_, i) => i + 1).map(p => (
              <button key={p} onClick={() => setPage(p)}
                className="w-8 h-8 flex items-center justify-center rounded border text-sm"
                style={{ borderColor: p === page ? B : G200, backgroundColor: p === page ? B : "white", color: p === page ? "white" : G900 }}>
                {p}
              </button>
            ))}
            <button onClick={() => setPage(p => Math.min(pages, p + 1))} disabled={page === pages}
              className="w-8 h-8 flex items-center justify-center rounded border text-sm disabled:opacity-30"
              style={{ borderColor: G200 }}>
              <ChevronRight size={13} />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

function AdminRoutes() {
  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-extrabold" style={{ color: G900 }}>Optimización de Rutas</h1>
          <p className="text-sm mt-0.5" style={{ color: G500 }}>4 cuadrillas activas · 9 órdenes en curso hoy</p>
        </div>
        <button className="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-bold text-white min-h-[40px]"
          style={{ backgroundColor: B }}>
          <RefreshCw size={14} /> Recalcular rutas
        </button>
      </div>

      <div className="grid grid-cols-3 gap-4">
        <div className="col-span-2 bg-white rounded-2xl shadow-sm border overflow-hidden" style={{ borderColor: G200 }}>
          <div className="px-4 py-3 border-b flex items-center justify-between" style={{ borderColor: "#F0F1F4" }}>
            <h3 className="font-bold text-sm" style={{ color: G900 }}>Mapa de rutas activas — Santo Domingo</h3>
            <div className="flex gap-4 text-xs" style={{ color: G500 }}>
              {[{ color: T, label: "Activo" }, { color: Y, label: "En tránsito" }, { color: "#9CA3AF", label: "Inactivo" }].map((l, i) => (
                <span key={i} className="flex items-center gap-1.5">
                  <span className="w-2 h-2 rounded-full" style={{ backgroundColor: l.color }} />{l.label}
                </span>
              ))}
            </div>
          </div>
          <CityMapSVG activeLayers={new Set(ISSUE_TYPES.map(t => t.id))} showHeatmap={false} svgH={420} />
        </div>

        <div className="space-y-3">
          {CREWS.map(crew => (
            <div key={crew.id} className="bg-white rounded-2xl p-4 shadow-sm border" style={{ borderColor: G200 }}>
              <div className="flex items-start justify-between mb-2">
                <div>
                  <p className="font-bold text-sm" style={{ color: G900 }}>{crew.name}</p>
                  <p className="text-xs" style={{ color: G500 }}>{crew.district}</p>
                </div>
                <span className="text-xs font-semibold px-2 py-0.5 rounded-full"
                  style={{
                    backgroundColor: crew.status === "active" ? T + "18" : crew.status === "transit" ? Y + "22" : "#F3F4F6",
                    color: crew.status === "active" ? T : crew.status === "transit" ? "#856404" : "#9CA3AF",
                  }}>
                  {crew.status === "active" ? "Activo" : crew.status === "transit" ? "En tránsito" : "Inactivo"}
                </span>
              </div>
              <div className="flex gap-3 text-xs" style={{ color: G500 }}>
                <span className="flex items-center gap-1"><Users size={11} />{crew.members} miembros</span>
                <span className="flex items-center gap-1"><FileText size={11} />{crew.orders} órdenes</span>
              </div>
              {crew.dist !== "—" && (
                <div className="mt-2 pt-2 border-t flex items-center gap-1 text-xs" style={{ borderColor: "#F0F1F4", color: B }}>
                  <Navigation size={11} /> Distancia total: {crew.dist}
                </div>
              )}
            </div>
          ))}

          <div className="bg-white rounded-2xl p-4 shadow-sm border" style={{ borderColor: G200 }}>
            <h4 className="font-bold text-sm mb-3" style={{ color: G900 }}>Métricas de eficiencia</h4>
            <div className="space-y-2.5">
              {[
                { label: "Distancia ahorrada", value: "23%",  color: T },
                { label: "Tiempo estimado total", value: "4.2h", color: B },
                { label: "Ahorro combustible",  value: "−18L", color: T },
              ].map((m, i) => (
                <div key={i} className="flex items-center justify-between">
                  <span className="text-xs" style={{ color: G500 }}>{m.label}</span>
                  <span className="text-sm font-bold" style={{ color: m.color }}>{m.value}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

function AdminModeration() {
  const [queue, setQueue] = useState(MODERATION);
  const act = (id: string) => setQueue(q => q.filter(r => r.id !== id));

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-extrabold" style={{ color: G900 }}>Cola de Moderación</h1>
          <p className="text-sm mt-0.5" style={{ color: G500 }}>{queue.length} reporte{queue.length !== 1 ? "s" : ""} pendiente{queue.length !== 1 ? "s" : ""} de revisión</p>
        </div>
        <div className="flex gap-2">
          <button className="flex items-center gap-2 px-3 py-2 rounded-lg border text-sm min-h-[40px]"
            style={{ borderColor: G200 }}><Filter size={13} /> Filtrar</button>
          <button className="flex items-center gap-2 px-3 py-2 rounded-lg border text-sm min-h-[40px]"
            style={{ borderColor: G200 }}><RefreshCw size={13} /> Actualizar</button>
        </div>
      </div>

      {queue.length === 0 ? (
        <div className="bg-white rounded-2xl p-16 text-center border" style={{ borderColor: G200 }}>
          <CheckCircle size={52} className="mx-auto mb-4" style={{ color: T }} />
          <h3 className="font-bold text-xl mb-1" style={{ color: G900 }}>¡Cola vacía!</h3>
          <p style={{ color: G500 }}>Todos los reportes han sido moderados.</p>
        </div>
      ) : (
        <div className="space-y-3">
          {queue.map(report => (
            <div key={report.id} className="bg-white rounded-2xl shadow-sm border overflow-hidden"
              style={{ borderColor: report.escalated ? R + "50" : G200 }}>
              {report.escalated && (
                <div className="px-4 py-2 flex items-center gap-2 text-xs font-bold"
                  style={{ backgroundColor: R + "12", color: R }}>
                  <AlertTriangle size={12} />
                  Reporte escalado — Alta incidencia ciudadana ({report.votes} votos ciudadanos)
                </div>
              )}
              <div className="p-5">
                <div className="flex items-start gap-4">
                  <div className="w-12 h-12 rounded-xl flex items-center justify-center text-2xl flex-shrink-0"
                    style={{ backgroundColor: "#F3F4F6" }}>
                    {ISSUE_TYPES.find(t => t.id === report.type)?.emoji ?? "📍"}
                  </div>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                      <span className="font-mono text-xs font-bold" style={{ color: B }}>{report.id}</span>
                      <PriorityBadge p={report.priority} />
                      <span className="text-xs" style={{ color: G500 }}>{report.created}</span>
                    </div>
                    <p className="font-bold text-sm mb-0.5" style={{ color: G900 }}>
                      {ISSUE_TYPES.find(t => t.id === report.type)?.label ?? "Reporte"} — {report.address}
                    </p>
                    <p className="text-xs" style={{ color: G500 }}>
                      Reportado por: {report.reporter} · {report.votes} votos ciudadanos
                    </p>
                  </div>
                  <div className="flex gap-2 flex-shrink-0">
                    <button onClick={() => act(report.id)}
                      className="flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm font-semibold min-h-[40px]"
                      style={{ backgroundColor: T + "15", color: T }}>
                      <Check size={14} /> Aprobar
                    </button>
                    {report.escalated && (
                      <button onClick={() => act(report.id)}
                        className="flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm font-semibold min-h-[40px]"
                        style={{ backgroundColor: R + "15", color: R }}>
                        <Phone size={14} /> Escalar 911
                      </button>
                    )}
                    <button onClick={() => act(report.id)}
                      className="flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm font-semibold min-h-[40px]"
                      style={{ backgroundColor: "#F3F4F6", color: G500 }}>
                      <X size={14} /> Rechazar
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function AdminDashboard() {
  const [screen, setScreen] = useState<AdminTab>("kpis");
  const [collapsed, setCollapsed] = useState(false);

  return (
    <div className="flex h-full">
      {/* Sidebar */}
      <aside className="flex flex-col border-r transition-all duration-200 flex-shrink-0"
        style={{ width: collapsed ? 64 : 240, backgroundColor: "#0B1629", borderColor: "rgba(255,255,255,0.08)" }}>
        <div className="flex items-center gap-3 px-4 py-4 border-b" style={{ borderColor: "rgba(255,255,255,0.08)" }}>
          <div className="w-8 h-8 rounded-lg flex items-center justify-center text-lg flex-shrink-0"
            style={{ backgroundColor: B }}>🏙️</div>
          {!collapsed && (
            <div className="flex-1 min-w-0">
              <p className="text-white font-bold text-sm leading-tight">UrbanSync</p>
              <p className="text-xs" style={{ color: "rgba(255,255,255,0.45)" }}>Administración · ADN</p>
            </div>
          )}
          <button onClick={() => setCollapsed(c => !c)}
            className="flex-shrink-0 p-1 rounded transition-colors"
            style={{ color: "rgba(255,255,255,0.45)" }}
            aria-label={collapsed ? "Expandir menú lateral" : "Colapsar menú lateral"}>
            {collapsed ? <ChevronRight size={16} /> : <ChevronLeft size={16} />}
          </button>
        </div>

        <nav className="flex-1 py-3 px-2 space-y-0.5">
          {ADMIN_NAV.map(item => {
            const active = screen === item.id;
            return (
              <button key={item.id} onClick={() => setScreen(item.id)}
                className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all min-h-[44px] text-left"
                style={{
                  backgroundColor: active ? B : "transparent",
                  color: active ? "white" : "rgba(255,255,255,0.55)",
                }}
                aria-current={active ? "page" : undefined}
                title={collapsed ? item.label : undefined}>
                <item.Icon size={18} style={{ flexShrink: 0 }} />
                {!collapsed && <span className="text-sm font-medium truncate">{item.label}</span>}
              </button>
            );
          })}
        </nav>

        <div className="px-3 py-3 border-t" style={{ borderColor: "rgba(255,255,255,0.08)" }}>
          <div className={`flex items-center gap-2.5 ${collapsed ? "justify-center" : ""}`}>
            <div className="w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold text-white flex-shrink-0"
              style={{ backgroundColor: T }}>DG</div>
            {!collapsed && (
              <div className="flex-1 min-w-0">
                <p className="text-xs font-semibold text-white truncate">Dirección General</p>
                <p className="text-xs truncate" style={{ color: "rgba(255,255,255,0.45)" }}>Admin · ADN</p>
              </div>
            )}
          </div>
        </div>
      </aside>

      {/* Main */}
      <div className="flex-1 flex flex-col min-w-0">
        <header className="flex items-center gap-4 px-6 py-3 bg-white border-b" style={{ borderColor: G200 }}>
          <div className="flex items-center gap-2 flex-1 max-w-md px-3 py-2 rounded-lg" style={{ backgroundColor: G50 }}>
            <Search size={14} className="text-gray-400" />
            <span className="text-sm" style={{ color: "#ADADBD" }}>Buscar activos, órdenes, reportes...</span>
          </div>
          <div className="ml-auto flex items-center gap-1">
            <button className="relative w-10 h-10 flex items-center justify-center rounded-lg hover:bg-gray-50"
              aria-label="Notificaciones — 3 nuevas">
              <Bell size={17} style={{ color: "#374151" }} />
              <span className="absolute top-2 right-2 w-2 h-2 rounded-full" style={{ backgroundColor: R }} />
            </button>
            <button className="w-10 h-10 flex items-center justify-center rounded-lg hover:bg-gray-50"
              aria-label="Configuración">
              <Settings size={17} style={{ color: "#374151" }} />
            </button>
          </div>
        </header>

        <main className="flex-1 overflow-y-auto p-6" style={{ backgroundColor: G50 }}>
          {screen === "kpis"       && <AdminKPIs />}
          {screen === "map"        && <AdminMapView />}
          {screen === "tables"     && <AdminTables />}
          {screen === "routes"     && <AdminRoutes />}
          {screen === "moderation" && <AdminModeration />}
        </main>
      </div>
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════════
// ROOT APP
// ═══════════════════════════════════════════════════════════════════════════

const ROLES = [
  { id: "citizen",    label: "Portal Ciudadano",  sub: "Móvil · Responsive",    emoji: "👤" },
  { id: "technician", label: "Técnico de Campo",  sub: "Móvil · Offline-first", emoji: "👷" },
  { id: "admin",      label: "Administración",     sub: "Dashboard · Escritorio", emoji: "🏛️" },
] as const;

export default function App() {
  const [role, setRole] = useState<Role>("citizen");
  const [highContrast, setHighContrast] = useState(false);
  const [largeText, setLargeText] = useState(false);
  const isMobile = role !== "admin";

  return (
    <div className="min-h-screen flex flex-col"
      style={{ backgroundColor: "#D6DBE4", fontSize: largeText ? 18 : undefined, filter: highContrast ? "contrast(1.2) saturate(1.1)" : undefined }}>

      {/* Top bar */}
      <header className="sticky top-0 z-50 bg-white border-b" style={{ borderColor: G200 }}>
        <div className="flex items-center gap-3 px-4 py-2.5 max-w-[1440px] mx-auto">
          {/* Logo */}
          <div className="flex items-center gap-2.5 flex-shrink-0">
            <div className="w-9 h-9 rounded-xl flex items-center justify-center text-lg shadow-sm" style={{ backgroundColor: B }}>🏙️</div>
            <div className="hidden sm:block">
              <p className="font-extrabold text-sm leading-tight" style={{ color: G900 }}>UrbanSync</p>
              <p className="text-xs" style={{ color: G500 }}>Santo Domingo, R.D.</p>
            </div>
          </div>

          {/* Role switcher */}
          <div className="flex items-center gap-1 mx-4 bg-gray-100 p-1 rounded-xl flex-1 max-w-lg">
            {ROLES.map(r => (
              <button key={r.id} onClick={() => setRole(r.id)}
                className="flex-1 flex items-center justify-center gap-1.5 py-2 px-2 rounded-lg text-xs font-bold transition-all min-h-[40px]"
                style={{
                  backgroundColor: role === r.id ? B : "transparent",
                  color: role === r.id ? "white" : G500,
                  boxShadow: role === r.id ? `0 1px 4px ${B}50` : "none",
                }}>
                <span>{r.emoji}</span>
                <span className="hidden sm:inline">{r.label}</span>
              </button>
            ))}
          </div>

          {/* Accessibility bar */}
          <div className="ml-auto flex items-center gap-1.5">
            <button onClick={() => setHighContrast(c => !c)}
              className="flex items-center gap-1.5 px-3 py-2 rounded-lg text-xs font-semibold min-h-[40px] border transition-all"
              style={{
                backgroundColor: highContrast ? G900 : "white",
                color: highContrast ? "white" : G500,
                borderColor: highContrast ? G900 : G200,
              }}
              aria-pressed={highContrast}>
              <Sun size={14} />
              <span className="hidden md:inline">Alto contraste</span>
            </button>
            <button onClick={() => setLargeText(c => !c)}
              className="flex items-center gap-1.5 px-3 py-2 rounded-lg text-xs font-semibold min-h-[40px] border transition-all"
              style={{
                backgroundColor: largeText ? B : "white",
                color: largeText ? "white" : G500,
                borderColor: largeText ? B : G200,
              }}
              aria-pressed={largeText}>
              <Type size={14} />
              <span className="hidden md:inline">Texto grande</span>
            </button>
          </div>
        </div>
        <div className="px-4 py-1.5 border-t max-w-[1440px] mx-auto" style={{ borderColor: "#F0F1F4" }}>
          <p className="text-xs" style={{ color: G500 }}>
            Visualizando: <strong style={{ color: G900 }}>{ROLES.find(r => r.id === role)?.label}</strong>
            {" · "}{ROLES.find(r => r.id === role)?.sub}
            {" · "}
            <span style={{ color: T }}>WCAG 2.1 AA</span>
          </p>
        </div>
      </header>

      {/* Content */}
      <main className="flex-1 flex items-start justify-center p-6">
        {isMobile ? (
          <div className="w-full max-w-[390px]">
            {/* Device frame */}
            <div className="rounded-[42px] shadow-2xl overflow-hidden"
              style={{ border: "6px solid #222", backgroundColor: "#1A1A1A" }}>
              {/* Status bar */}
              <div className="flex items-center justify-between px-7 py-2.5 text-white text-xs font-semibold"
                style={{ backgroundColor: "#1A1A1A" }}>
                <span>9:41</span>
                <div className="w-20 h-5 rounded-full" style={{ backgroundColor: "#111" }} />
                <div className="flex items-center gap-1 text-white text-xs">
                  <span className="font-bold">●●●</span>
                  <span>92%</span>
                </div>
              </div>
              {/* Screen */}
              <div style={{ height: 780, borderRadius: "32px 32px 0 0", overflow: "hidden" }}>
                {role === "citizen"    && <CitizenPortal />}
                {role === "technician" && <TechnicianApp />}
              </div>
            </div>
            <p className="text-center text-xs mt-4" style={{ color: G500 }}>
              {ROLES.find(r => r.id === role)?.sub} · Interactivo
            </p>
          </div>
        ) : (
          <div className="w-full" style={{ maxWidth: 1280, height: 768 }}>
            <div className="rounded-2xl overflow-hidden shadow-2xl border" style={{ borderColor: G200, height: "100%" }}>
              <AdminDashboard />
            </div>
            <p className="text-center text-xs mt-4" style={{ color: G500 }}>
              Dashboard de Administración · Escritorio · Interactivo
            </p>
          </div>
        )}
      </main>
    </div>
  );
}
