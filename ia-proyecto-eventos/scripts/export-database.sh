#!/bin/bash
# Script para exportar la base de datos MySQL desde Docker

# Variables
CONTAINER_NAME="ia-proyecto-mysql"
DB_NAME="ia_proyecto_eventos"
DB_USER="iauser"
DB_PASSWORD="iapassword"
BACKUP_DIR="./backups"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILE="$BACKUP_DIR/${DB_NAME}_backup_${TIMESTAMP}.sql"

# Crear directorio de backups si no existe
mkdir -p $BACKUP_DIR

echo "Exportando base de datos..."

# Exportar con mysqldump desde el contenedor
docker exec $CONTAINER_NAME mysqldump -u$DB_USER -p$DB_PASSWORD $DB_NAME > $BACKUP_FILE

if [ $? -eq 0 ]; then
    echo "Base de datos exportada exitosamente a: $BACKUP_FILE"
    
    # Comprimir el backup
    gzip $BACKUP_FILE
    echo "Backup comprimido: ${BACKUP_FILE}.gz"
else
    echo "Error al exportar la base de datos"
    exit 1
fi
