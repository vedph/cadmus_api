FROM mongo

COPY cadmuslex.tar.gz /cadmuslex.tar.gz
CMD mongorestore --host mongodb://mongo:27017 --archive=/cadmuslex.tar.gz --gzip