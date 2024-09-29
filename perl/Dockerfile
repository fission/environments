FROM perl:5.41.4-bookworm

RUN cpanm -n Twiggy Getopt::Args Dancer2

COPY    server.pl /server.pl
WORKDIR /

EXPOSE 8888
ENTRYPOINT ["/server.pl"]
