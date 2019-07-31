FROM jenkins
COPY zscaler.pem /usr/local/share/ca-certificates/zscaler.crt
RUN update-ca-certificates
COPY ./plugins.txt /plugins.txt
RUN /usr/local/bin/install-plugins.sh /plugins.txt