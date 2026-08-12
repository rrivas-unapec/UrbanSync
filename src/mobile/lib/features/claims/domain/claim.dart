class Claim {
  const Claim({
    required this.id,
    required this.ciudadanoId,
    required this.ciudadano,
    required this.ubicacionId,
    required this.ubicacionDireccion,
    required this.categoria,
    required this.titulo,
    required this.descripcion,
    required this.estado,
    required this.fechaCreacion,
  });

  final int id;
  final int ciudadanoId;
  final String ciudadano;
  final int ubicacionId;
  final String ubicacionDireccion;
  final String categoria;
  final String titulo;
  final String descripcion;
  final String estado;
  final DateTime fechaCreacion;

  factory Claim.fromJson(Map<String, dynamic> json) => Claim(
    id: (json['id'] as num).toInt(),
    ciudadanoId: (json['citizenUserId'] as num?)?.toInt() ?? 0,
    ciudadano: json['citizenUserName'] as String? ?? '',
    ubicacionId: (json['locationId'] as num?)?.toInt() ?? 0,
    ubicacionDireccion: json['locationAddress'] as String? ?? '',
    categoria: json['category'] as String? ?? '',
    titulo: json['title'] as String? ?? '',
    descripcion: json['description'] as String? ?? '',
    estado: json['status'] as String? ?? '',
    fechaCreacion: DateTime.parse(json['createdAt'] as String),
  );
}
