FROM node:22-alpine

RUN npm install --global @bitwarden/cli@2026.10.0

COPY bw-gateway.sh /opt/gateway/bw-gateway.sh
RUN chmod +x /opt/gateway/bw-gateway.sh

EXPOSE 8087

ENTRYPOINT ["/opt/gateway/bw-gateway.sh"]
